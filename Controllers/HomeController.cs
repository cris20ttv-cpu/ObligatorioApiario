using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObligatorioApiario.Models;
using ObligatorioApiario.Data;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;

namespace ObligatorioApiario.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }



    public async Task<IActionResult> Index()
    {
        var viewModel = new DashboardViewModel();

        // Totales
        viewModel.TotalApiarios = await _context.Apiarios.CountAsync();
        viewModel.TotalColmenas = await _context.Colmenas.CountAsync();
        
        // Suma de Miel (Solo Última Cosecha) y Estimado
        var totalKilosHistorico = await _context.Barriles.SumAsync(b => b.Cantidad_Miel);
        var totalRealiza = await _context.Realiza.CountAsync();
        
        if (totalRealiza > 0 && viewModel.TotalApiarios > 0)
        {
            var promedioPorApiario = totalKilosHistorico / totalRealiza;
            viewModel.EstimadoProximaCosecha = promedioPorApiario * viewModel.TotalApiarios;
        }
        else
        {
            viewModel.EstimadoProximaCosecha = 0;
        }

        var ultimaFecha = await _context.Realiza
            .OrderByDescending(r => r.Fecha_Cosecha)
            .Select(r => r.Fecha_Cosecha)
            .FirstOrDefaultAsync();

        if (ultimaFecha != default(DateTime))
        {
            var fechaInicio = ultimaFecha.AddDays(-30);
            
            var cosechasDelMes = await _context.Realiza
                .Where(r => r.Fecha_Cosecha >= fechaInicio && r.Fecha_Cosecha <= ultimaFecha)
                .Select(r => r.ID_Cosecha)
                .Distinct()
                .ToListAsync();

            viewModel.TotalKilosMiel = await _context.Genera
                .Where(g => cosechasDelMes.Contains(g.ID_Cosecha))
                .SumAsync(g => g.Barril != null ? g.Barril.Cantidad_Miel : 0);
        }
        else
        {
            viewModel.TotalKilosMiel = 0;
        }

        // Visitas Pendientes
        viewModel.VisitasPendientes = await _context.Visitas
            .OrderByDescending(v => v.Fecha)
            .Take(5)
            .ToListAsync();

        // Datos para el gráfico: Agrupar cosechas por Mes/Año
        var cosechasConFecha = await _context.Realiza
            .Include(r => r.Cosecha)
                .ThenInclude(c => c.GeneraBarriles)
                    .ThenInclude(g => g.Barril)
            .OrderBy(r => r.Fecha_Cosecha)
            .ToListAsync();

        var produccionAgrupada = cosechasConFecha
            .GroupBy(r => r.ID_Cosecha)
            .Select(g => new 
            {
                Fecha = g.First().Fecha_Cosecha,
                Cosecha = g.First().Cosecha
            })
            .GroupBy(c => new { c.Fecha.Year, c.Fecha.Month })
            .Select(g => new ProduccionMensualData
            {
                Mes = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yy", new CultureInfo("es-ES")).ToUpper(),
                TotalKg = g.SelectMany(c => c.Cosecha?.GeneraBarriles ?? new List<Genera>())
                           .Sum(gen => gen.Barril?.Cantidad_Miel ?? 0)
            })
            .ToList();

        if (!produccionAgrupada.Any())
        {
            produccionAgrupada.Add(new ProduccionMensualData { Mes = DateTime.Now.AddMonths(-1).ToString("MMM yy", new CultureInfo("es-ES")).ToUpper(), TotalKg = 0 });
            produccionAgrupada.Add(new ProduccionMensualData { Mes = DateTime.Now.ToString("MMM yy", new CultureInfo("es-ES")).ToUpper(), TotalKg = 0 });
        }

        viewModel.GraficoProduccion = produccionAgrupada;

        // Apiarios para el Mapa
        viewModel.MapaApiarios = await _context.Apiarios
            .Where(a => a.Latitud != 0 || a.Longitud != 0) 
            .Select(a => new ApiarioUbicacionData
            {
                Id = a.ID_Apiario,
                Nombre = a.Nombre,
                Latitud = a.Latitud,
                Longitud = a.Longitud,
                Zona = a.Zona
            })
            .ToListAsync();

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
