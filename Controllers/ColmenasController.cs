using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ObligatorioApiario.Data;
using ObligatorioApiario.Models;

namespace ObligatorioApiario.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class ColmenasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ColmenasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Colmenas/Create
        public IActionResult Create(int? apiarioId)
        {
            if (apiarioId == null)
            {
                return NotFound();
            }

            var apiario = _context.Apiarios.Find(apiarioId);
            if (apiario == null)
            {
                return NotFound();
            }

            ViewBag.ApiarioNombre = apiario.Nombre;
            ViewBag.ApiarioId = apiarioId;
            
            var colmena = new Colmena
            {
                Estado = "Saludable",
                Cant_Bastidores = 10,
                Cant_Abejas_Estimada = 50000,
                Fortaleza_Abejas = "Media"
            };

            return View(colmena);
        }

        // POST: Colmenas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID_Colmena,Identificador,Estado,Cant_Bastidores,Cant_Abejas_Estimada,Fortaleza_Abejas")] Colmena colmena, int apiarioId)
        {
            if (ModelState.IsValid)
            {
                var colmenasExistentes = await _context.Colmenas.ToListAsync();
                int maxNumero = 0;
                
                foreach (var c in colmenasExistentes)
                {
                    if (!string.IsNullOrEmpty(c.Identificador) && c.Identificador.StartsWith("COL-"))
                    {
                        var numeroString = c.Identificador.Substring(4);
                        if (int.TryParse(numeroString, out int numero) && numero > maxNumero)
                        {
                            maxNumero = numero;
                        }
                    }
                }
                
                colmena.Identificador = $"COL-{maxNumero + 1:000}";

                _context.Add(colmena);
                await _context.SaveChangesAsync();

                // Asociar al Apiario mediante la entidad asociativa 'Tiene'
                var tiene = new Tiene
                {
                    ID_Apiario = apiarioId,
                    ID_Colmena = colmena.ID_Colmena,
                    Fecha_Instalacion = DateTime.Now,
                    Sector = "A",
                    Fila = "1",
                    Columna = "1"
                };
                _context.Tiene.Add(tiene);
                
                // Actualizar Cant_Colmenas en Apiarios
                var apiario = await _context.Apiarios.FindAsync(apiarioId);
                if(apiario != null)
                {
                    apiario.Cant_Colmenas++;
                    _context.Update(apiario);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Apiarios", new { id = apiarioId });
            }
            
            var ap = await _context.Apiarios.FindAsync(apiarioId);
            ViewBag.ApiarioNombre = ap?.Nombre;
            ViewBag.ApiarioId = apiarioId;
            return View(colmena);
        }

        public class HistorialMielDTO
        {
            public string IdentificadorCosecha { get; set; }
            public DateTime Fecha { get; set; }
            public string ApiarioNombre { get; set; }
            public decimal Kilos { get; set; }
        }

        // GET: Colmenas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var colmena = await _context.Colmenas
                .Include(c => c.Instalaciones)
                    .ThenInclude(t => t.Apiario)
                .Include(c => c.TratamientosAplicados)
                    .ThenInclude(ta => ta.Tratamiento)
                .Include(c => c.ReinasAsignadas)
                    .ThenInclude(ra => ra.Reina)
                .FirstOrDefaultAsync(m => m.ID_Colmena == id);
                
            if (colmena == null)
            {
                return NotFound();
            }

            var instalacionActual = colmena.Instalaciones.OrderByDescending(t => t.Fecha_Instalacion).FirstOrDefault();
            ViewBag.ApiarioActual = instalacionActual?.Apiario;
            ViewBag.FechaInstalacion = instalacionActual?.Fecha_Instalacion;

            // Historial de producción de miel
            var aportes = await _context.Aporta
                .Include(a => a.Cosecha)
                    .ThenInclude(c => c.RealizadaEn)
                        .ThenInclude(r => r.Apiario)
                .Where(a => a.ID_Colmena == id)
                .ToListAsync();

            var historialAgrupado = aportes
                .GroupBy(a => a.Cosecha)
                .Select(g => new HistorialMielDTO {
                    IdentificadorCosecha = g.Key?.Identificador ?? "N/A",
                    Fecha = g.Key?.RealizadaEn.FirstOrDefault()?.Fecha_Cosecha ?? DateTime.MinValue,
                    ApiarioNombre = g.Key?.RealizadaEn.FirstOrDefault()?.Apiario?.Nombre ?? "Desconocido",
                    Kilos = g.Sum(a => a.Kilos)
                })
                .OrderByDescending(x => x.Fecha)
                .ToList();

            ViewBag.HistorialMiel = historialAgrupado;

            return View(colmena);
        }

        // GET: Colmenas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var colmena = await _context.Colmenas.FindAsync(id);
            if (colmena == null)
            {
                return NotFound();
            }
            
            var tiene = await _context.Tiene.Where(t => t.ID_Colmena == id).OrderByDescending(t => t.Fecha_Instalacion).FirstOrDefaultAsync();
            int apiarioId = tiene?.ID_Apiario ?? 0;
            
            var apiario = await _context.Apiarios.FindAsync(apiarioId);
            ViewBag.ApiarioNombre = apiario?.Nombre;
            ViewBag.ApiarioId = apiarioId;
            ViewBag.ApiariosList = new SelectList(await _context.Apiarios.ToListAsync(), "ID_Apiario", "Nombre", apiarioId);

            return View(colmena);
        }

        // POST: Colmenas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID_Colmena,Identificador,Estado,Cant_Bastidores,Cant_Abejas_Estimada,Fortaleza_Abejas")] Colmena colmena, int apiarioId)
        {
            if (id != colmena.ID_Colmena)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(colmena);
                    
                    var instalacionActual = await _context.Tiene
                        .Where(t => t.ID_Colmena == colmena.ID_Colmena)
                        .OrderByDescending(t => t.Fecha_Instalacion)
                        .FirstOrDefaultAsync();

                    if (instalacionActual == null || instalacionActual.ID_Apiario != apiarioId)
                    {
                        var registroExistente = await _context.Tiene.FirstOrDefaultAsync(t => t.ID_Colmena == colmena.ID_Colmena && t.ID_Apiario == apiarioId);
                        if (registroExistente != null)
                        {
                            registroExistente.Fecha_Instalacion = DateTime.UtcNow;
                            _context.Update(registroExistente);
                        }
                        else
                        {
                            var nuevoTiene = new Tiene
                            {
                                ID_Colmena = colmena.ID_Colmena,
                                ID_Apiario = apiarioId,
                                Fecha_Instalacion = DateTime.UtcNow,
                                Sector = "Sector Principal",
                                Fila = "1",
                                Columna = "1"
                            };
                            _context.Tiene.Add(nuevoTiene);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ColmenaExists(colmena.ID_Colmena))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Apiarios", new { id = apiarioId });
            }
            
            var apiario = await _context.Apiarios.FindAsync(apiarioId);
            ViewBag.ApiarioNombre = apiario?.Nombre;
            ViewBag.ApiarioId = apiarioId;
            return View(colmena);
        }

        private bool ColmenaExists(int id)
        {
            return _context.Colmenas.Any(e => e.ID_Colmena == id);
        }

        // GET: Colmenas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var colmena = await _context.Colmenas
                .Include(c => c.Instalaciones)
                    .ThenInclude(t => t.Apiario)
                .FirstOrDefaultAsync(m => m.ID_Colmena == id);
                
            if (colmena == null)
            {
                return NotFound();
            }

            return View(colmena);
        }

        // POST: Colmenas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var colmena = await _context.Colmenas.FindAsync(id);
            if (colmena != null)
            {
                var tiene = await _context.Tiene.FirstOrDefaultAsync(t => t.ID_Colmena == id);
                int apiarioId = tiene?.ID_Apiario ?? 0;
                
                if (tiene != null) {
                    _context.Tiene.Remove(tiene);
                }
                
                _context.Colmenas.Remove(colmena);
                
                // Actualizar Cant_Colmenas en Apiarios
                if (apiarioId != 0) {
                    var apiario = await _context.Apiarios.FindAsync(apiarioId);
                    if(apiario != null)
                    {
                        apiario.Cant_Colmenas--;
                        _context.Update(apiario);
                    }
                }
                
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Apiarios", new { id = apiarioId });
            }
            return RedirectToAction("Index", "Apiarios");
        }
    }
}
