using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObligatorioApiario.Data;

namespace ObligatorioApiario.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class BarrilesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BarrilesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Barriles
        public async Task<IActionResult> Index()
        {
            var barriles = await _context.Barriles
                .Include(b => b.GeneradoPor)
                    .ThenInclude(g => g.Cosecha)
                .Where(b => b.Estado == "En Inventario")
                .OrderByDescending(b => b.ID_Barril)
                .ToListAsync();

            return View(barriles);
        }

        // GET: Barriles/Vender
        public async Task<IActionResult> Vender()
        {
            var barriles = await _context.Barriles
                .Include(b => b.GeneradoPor)
                    .ThenInclude(g => g.Cosecha)
                .Where(b => b.Estado == "En Inventario")
                .OrderByDescending(b => b.ID_Barril)
                .ToListAsync();

            return View(barriles);
        }

        // POST: Barriles/Vender
        [HttpPost]
        public async Task<IActionResult> Vender(List<int> seleccionados)
        {
            if (seleccionados != null && seleccionados.Any())
            {
                var barrilesAActualizar = await _context.Barriles
                    .Where(b => seleccionados.Contains(b.ID_Barril))
                    .ToListAsync();

                foreach(var barril in barrilesAActualizar)
                {
                    barril.Estado = "Vendido";
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Barriles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var barril = await _context.Barriles
                .Include(b => b.GeneradoPor)
                    .ThenInclude(g => g.Cosecha)
                        .ThenInclude(c => c.RealizadaEn)
                            .ThenInclude(r => r.Apiario)
                .FirstOrDefaultAsync(m => m.ID_Barril == id);

            if (barril == null)
            {
                return NotFound();
            }

            var aportes = await _context.Aporta
                .Include(a => a.Colmena)
                .Where(a => a.ID_Barril == id)
                .ToListAsync();

            ViewBag.Aportes = aportes;

            return View(barril);
        }
    }
}
