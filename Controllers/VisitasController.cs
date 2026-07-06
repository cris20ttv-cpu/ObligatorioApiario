using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ObligatorioApiario.Data;
using ObligatorioApiario.Models;

namespace ObligatorioApiario.Controllers
{
    public class VisitasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VisitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Visitas
        public async Task<IActionResult> Index()
        {
            var visitas = await _context.Visitas
                .Include(v => v.Apicultor)
                .Include(v => v.Tareas)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            var pendientes = visitas.Where(v => !v.Tareas.Any() || v.Tareas.Any(t => t.Estado != "Completada")).ToList();
            var completadas = visitas.Where(v => v.Tareas.Any() && v.Tareas.All(t => t.Estado == "Completada")).ToList();

            ViewBag.Pendientes = pendientes;
            ViewBag.Completadas = completadas;

            return View(visitas);
        }

        // GET: Visitas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visita = await _context.Visitas
                .Include(v => v.Apicultor)
                .Include(v => v.ApiariosRevisados)
                    .ThenInclude(r => r.Apiario)
                .Include(v => v.HerramientasPlanificadas)
                    .ThenInclude(p => p.Herramienta)
                .Include(v => v.Tareas)
                    .ThenInclude(t => t.Colmena)
                .FirstOrDefaultAsync(m => m.ID_Visita == id);

            if (visita == null)
            {
                return NotFound();
            }

            // ViewBags for the inline forms
            ViewData["ID_Apiario"] = new SelectList(await _context.Apiarios.ToListAsync(), "ID_Apiario", "Nombre");
            ViewData["ID_Herramienta"] = new SelectList(await _context.Herramientas.ToListAsync(), "ID_Herramienta", "Nombre");

            return View(visita);
        }

        // DTOs para la creación de Visita
        public class TareaDto
        {
            public int ColmenaId { get; set; }
            public string Descripcion { get; set; } = string.Empty;
        }

        public class HerramientaDto
        {
            public string Nombre { get; set; } = string.Empty;
            public int Cantidad { get; set; }
        }

        public class CreateVisitaPayload
        {
            public int ApiarioId { get; set; }
            public int ApicultorId { get; set; }
            public DateTime Fecha { get; set; }
            public string Temporada { get; set; } = string.Empty;
            public string Observaciones { get; set; } = string.Empty;
            public List<HerramientaDto> Herramientas { get; set; } = new List<HerramientaDto>();
            public List<TareaDto> Tareas { get; set; } = new List<TareaDto>();
        }

        [HttpGet]
        public async Task<IActionResult> GetColmenasPorApiario(int apiarioId)
        {
            var historiales = await _context.Tiene.ToListAsync();
            var colmenasEnApiario = historiales
                .GroupBy(t => t.ID_Colmena)
                .Select(g => g.OrderByDescending(t => t.Fecha_Instalacion).FirstOrDefault())
                .Where(t => t != null && t.ID_Apiario == apiarioId)
                .Select(t => t.ID_Colmena)
                .ToList();

            var colmenas = await _context.Colmenas
                .Where(c => colmenasEnApiario.Contains(c.ID_Colmena))
                .Select(c => new { 
                    id = c.ID_Colmena, 
                    identificador = c.Identificador,
                    estado = c.Estado
                })
                .ToListAsync();

            return Json(colmenas);
        }

        [HttpPost]
        public async Task<IActionResult> CreateJson([FromBody] CreateVisitaPayload payload)
        {
            if (payload == null || payload.ApiarioId <= 0)
                return BadRequest("Datos inválidos");

            try
            {
                if (payload.Fecha.Date < DateTime.Today)
                {
                    return BadRequest(new { success = false, message = "La fecha de la visita no puede ser anterior a hoy." });
                }

                var visita = new Visita
                {
                    Fecha = DateTime.SpecifyKind(payload.Fecha, DateTimeKind.Utc),
                    Temporada = payload.Temporada,
                    Observaciones = payload.Observaciones,
                    CIApicultor = payload.ApicultorId
                };
                
                _context.Visitas.Add(visita);
                await _context.SaveChangesAsync();

                // Asociar Apiario
                var revisa = new Revisa
                {
                    ID_Visita = visita.ID_Visita,
                    ID_Apiario = payload.ApiarioId,
                    H_Salida = TimeSpan.Zero,
                    H_Llegada = TimeSpan.Zero,
                    Clima = "Despejado" // Default
                };
                _context.Revisa.Add(revisa);

                // Asociar Herramientas
                foreach(var h in payload.Herramientas)
                {
                    if (string.IsNullOrWhiteSpace(h.Nombre)) continue;
                    
                    var nombreBuscado = h.Nombre.Trim();
                    var herramientaExistente = await _context.Herramientas
                        .FirstOrDefaultAsync(x => x.Nombre.ToLower() == nombreBuscado.ToLower());
                    
                    int idHerramienta = 0;
                    if (herramientaExistente != null)
                    {
                        idHerramienta = herramientaExistente.ID_Herramienta;
                    }
                    else
                    {
                        var nuevaHerramienta = new Herramienta
                        {
                            Nombre = nombreBuscado,
                            Tipo = "General"
                        };
                        _context.Herramientas.Add(nuevaHerramienta);
                        await _context.SaveChangesAsync();
                        idHerramienta = nuevaHerramienta.ID_Herramienta;
                    }

                    _context.Planifica.Add(new Planifica
                    {
                        ID_Visita = visita.ID_Visita,
                        ID_Herramienta = idHerramienta,
                        Cantidad = h.Cantidad
                    });
                }

                // Asociar Tareas
                foreach(var t in payload.Tareas)
                {
                    _context.TareasVisita.Add(new TareaVisita
                    {
                        ID_Visita = visita.ID_Visita,
                        ID_Colmena = t.ColmenaId,
                        Descripcion = t.Descripcion,
                        Estado = "Pendiente"
                    });
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, url = Url.Action("Index", "Visitas") });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        // GET: Visitas/Create
        public IActionResult Create()
        {
            ViewData["CIApicultor"] = new SelectList(_context.Apicultores, "CIApicultor", "Nombre");
            ViewBag.Apiarios = _context.Apiarios.ToList();
            ViewBag.Herramientas = _context.Herramientas.ToList();
            return View();
        }

        // POST: Visitas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID_Visita,Fecha,Temporada,Observaciones,CIApicultor")] Visita visita)
        {
            if (visita.Fecha.Date < DateTime.Now.Date)
            {
                ModelState.AddModelError("Fecha", "No se puede programar una visita para una fecha pasada.");
            }

            if (ModelState.IsValid)
            {
                visita.Fecha = visita.Fecha.ToUniversalTime();
                _context.Add(visita);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = visita.ID_Visita });
            }
            ViewData["CIApicultor"] = new SelectList(_context.Apicultores, "CIApicultor", "Nombre", visita.CIApicultor);
            return View(visita);
        }

        // GET: Visitas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visita = await _context.Visitas
                .Include(v => v.ApiariosRevisados)
                .Include(v => v.HerramientasPlanificadas)
                    .ThenInclude(p => p.Herramienta)
                .Include(v => v.Tareas)
                .FirstOrDefaultAsync(m => m.ID_Visita == id);
                
            if (visita == null)
            {
                return NotFound();
            }
            // Convert to local time for the form
            visita.Fecha = visita.Fecha.ToLocalTime();
            ViewData["CIApicultor"] = new SelectList(_context.Apicultores, "CIApicultor", "Nombre", visita.CIApicultor);
            ViewBag.Apiarios = _context.Apiarios.ToList();
            ViewBag.Herramientas = _context.Herramientas.ToList();
            return View(visita);
        }

        // POST: Visitas/EditJson
        [HttpPost]
        public async Task<IActionResult> EditJson(int id, [FromBody] CreateVisitaPayload payload)
        {
            try
            {
                if (payload.Fecha.Date < DateTime.Today)
                {
                    return BadRequest(new { success = false, message = "La fecha de la visita no puede ser anterior a hoy." });
                }

                var visita = await _context.Visitas
                    .Include(v => v.ApiariosRevisados)
                    .Include(v => v.HerramientasPlanificadas)
                    .Include(v => v.Tareas)
                    .FirstOrDefaultAsync(v => v.ID_Visita == id);

                if (visita == null) return NotFound();

                visita.Fecha = DateTime.SpecifyKind(payload.Fecha, DateTimeKind.Utc);
                visita.Temporada = payload.Temporada;
                visita.Observaciones = payload.Observaciones;
                visita.CIApicultor = payload.ApicultorId;

                // Remover antiguos
                _context.Revisa.RemoveRange(visita.ApiariosRevisados);
                _context.Planifica.RemoveRange(visita.HerramientasPlanificadas);
                _context.TareasVisita.RemoveRange(visita.Tareas);

                // Agregar nuevo apiario
                _context.Revisa.Add(new Revisa
                {
                    ID_Visita = visita.ID_Visita,
                    ID_Apiario = payload.ApiarioId,
                    H_Salida = TimeSpan.Zero,
                    H_Llegada = TimeSpan.Zero,
                    Clima = "Despejado"
                });

                // Agregar nuevas herramientas
                foreach(var h in payload.Herramientas)
                {
                    if (string.IsNullOrWhiteSpace(h.Nombre)) continue;
                    var nombreBuscado = h.Nombre.Trim();
                    var herramientaExistente = await _context.Herramientas
                        .FirstOrDefaultAsync(x => x.Nombre.ToLower() == nombreBuscado.ToLower());
                    
                    int idHerramienta = 0;
                    if (herramientaExistente != null)
                    {
                        idHerramienta = herramientaExistente.ID_Herramienta;
                    }
                    else
                    {
                        var nuevaHerramienta = new Herramienta { Nombre = nombreBuscado, Tipo = "General" };
                        _context.Herramientas.Add(nuevaHerramienta);
                        await _context.SaveChangesAsync();
                        idHerramienta = nuevaHerramienta.ID_Herramienta;
                    }

                    _context.Planifica.Add(new Planifica
                    {
                        ID_Visita = visita.ID_Visita,
                        ID_Herramienta = idHerramienta,
                        Cantidad = h.Cantidad
                    });
                }

                // Agregar nuevas tareas
                foreach(var t in payload.Tareas)
                {
                    _context.TareasVisita.Add(new TareaVisita
                    {
                        ID_Visita = visita.ID_Visita,
                        ID_Colmena = t.ColmenaId,
                        Descripcion = t.Descripcion,
                        Estado = "Pendiente"
                    });
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, url = Url.Action("Details", "Visitas", new { id = visita.ID_Visita }) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleTareaStatus(int id)
        {
            var tarea = await _context.TareasVisita.FindAsync(id);
            if (tarea == null) return NotFound();

            tarea.Estado = (tarea.Estado == "Pendiente") ? "Completada" : "Pendiente";
            await _context.SaveChangesAsync();
            return Ok(new { success = true, newState = tarea.Estado });
        }

        // POST: Visitas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID_Visita,Fecha,Temporada,Observaciones,CIApicultor")] Visita visita)
        {
            if (id != visita.ID_Visita)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    visita.Fecha = visita.Fecha.ToUniversalTime();
                    _context.Update(visita);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VisitaExists(visita.ID_Visita))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CIApicultor"] = new SelectList(_context.Apicultores, "CIApicultor", "Nombre", visita.CIApicultor);
            return View(visita);
        }

        // GET: Visitas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visita = await _context.Visitas
                .Include(v => v.Apicultor)
                .FirstOrDefaultAsync(m => m.ID_Visita == id);
            if (visita == null)
            {
                return NotFound();
            }

            return View(visita);
        }

        // POST: Visitas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var visita = await _context.Visitas.FindAsync(id);
            if (visita != null)
            {
                _context.Visitas.Remove(visita);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Visitas/AddApiario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddApiario(int ID_Visita, int ID_Apiario, TimeSpan H_Salida, TimeSpan H_Llegada, string Clima)
        {
            var visitaExists = await _context.Visitas.AnyAsync(v => v.ID_Visita == ID_Visita);
            var apiarioExists = await _context.Apiarios.AnyAsync(a => a.ID_Apiario == ID_Apiario);
            
            if (!visitaExists || !apiarioExists) return NotFound();

            var revisa = new Revisa
            {
                ID_Visita = ID_Visita,
                ID_Apiario = ID_Apiario,
                H_Salida = H_Salida,
                H_Llegada = H_Llegada,
                Clima = Clima
            };

            _context.Revisa.Add(revisa);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Details), new { id = ID_Visita });
        }
        
        // POST: Visitas/RemoveApiario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveApiario(int ID_Visita, int ID_Apiario)
        {
            var revisa = await _context.Revisa.FirstOrDefaultAsync(r => r.ID_Visita == ID_Visita && r.ID_Apiario == ID_Apiario);
            if (revisa != null)
            {
                _context.Revisa.Remove(revisa);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id = ID_Visita });
        }

        // POST: Visitas/AddHerramienta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddHerramienta(int ID_Visita, int ID_Herramienta, int Cantidad)
        {
            var visitaExists = await _context.Visitas.AnyAsync(v => v.ID_Visita == ID_Visita);
            var herramientaExists = await _context.Herramientas.AnyAsync(h => h.ID_Herramienta == ID_Herramienta);
            
            if (!visitaExists || !herramientaExists) return NotFound();

            var planifica = new Planifica
            {
                ID_Visita = ID_Visita,
                ID_Herramienta = ID_Herramienta,
                Cantidad = Cantidad
            };

            _context.Planifica.Add(planifica);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Details), new { id = ID_Visita });
        }

        // POST: Visitas/RemoveHerramienta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveHerramienta(int ID_Visita, int ID_Herramienta)
        {
            var planifica = await _context.Planifica.FirstOrDefaultAsync(p => p.ID_Visita == ID_Visita && p.ID_Herramienta == ID_Herramienta);
            if (planifica != null)
            {
                _context.Planifica.Remove(planifica);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id = ID_Visita });
        }


        private bool VisitaExists(int id)
        {
            return _context.Visitas.Any(e => e.ID_Visita == id);
        }
    }
}
