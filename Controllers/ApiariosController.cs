using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObligatorioApiario.Data;
using ObligatorioApiario.Models;

namespace ObligatorioApiario.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class ApiariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApiariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var apiarios = _context.Apiarios
                .Include(a => a.ColmenasInstaladas)
                    .ThenInclude(t => t.Colmena)
                .Include(a => a.CosechasRealizadas)
                    .ThenInclude(r => r.Cosecha)
                .ToList();

            var todosLosTiene = _context.Tiene.ToList();
            
            foreach(var apiario in apiarios)
            {
                if (apiario.ColmenasInstaladas != null)
                {
                    apiario.ColmenasInstaladas = apiario.ColmenasInstaladas.Where(t => 
                    {
                        var ultimo = todosLosTiene.Where(h => h.ID_Colmena == t.ID_Colmena).OrderByDescending(h => h.Fecha_Instalacion).FirstOrDefault();
                        return ultimo != null && ultimo.ID_Apiario == apiario.ID_Apiario;
                    }).ToList();
                }
            }

            var produccionPorApiario = new Dictionary<int, decimal>();
            foreach (var apiario in apiarios)
            {
                var ultimaRealiza = apiario.CosechasRealizadas?
                    .OrderByDescending(r => r.ID_Cosecha)
                    .FirstOrDefault();
                if (ultimaRealiza != null)
                {
                    var countApiariosEnCosecha = _context.Realiza.Count(r => r.ID_Cosecha == ultimaRealiza.ID_Cosecha);
                    
                    if (countApiariosEnCosecha == 1)
                    {
                        // Es una cosecha creada por el usuario para este apiario específico
                        var totalKilos = _context.Genera
                            .Where(g => g.ID_Cosecha == ultimaRealiza.ID_Cosecha)
                            .Include(g => g.Barril)
                            .Sum(g => (decimal?)g.Barril.Cantidad_Miel) ?? 0;
                            
                        produccionPorApiario[apiario.ID_Apiario] = totalKilos;
                    }
                    else
                    {
                        // Es una cosecha masiva generada por el seeder, usamos la estimación
                        produccionPorApiario[apiario.ID_Apiario] = ultimaRealiza.Cant_Barriles * 300;
                    }
                }
                else
                {
                    produccionPorApiario[apiario.ID_Apiario] = 0;
                }
            }
            ViewBag.ProduccionPorApiario = produccionPorApiario;

            return View(apiarios);
        }

        // GET: Apiarios/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiario = _context.Apiarios
                .Include(a => a.ColmenasInstaladas)
                    .ThenInclude(t => t.Colmena)
                .Include(a => a.VisitasRecibidas)
                    .ThenInclude(r => r.Visita)
                .FirstOrDefault(m => m.ID_Apiario == id);
                
            if (apiario == null)
            {
                return NotFound();
            }

            if (apiario.ColmenasInstaladas != null)
            {
                var colmenasIds = apiario.ColmenasInstaladas.Select(t => t.ID_Colmena).ToList();
                var historiales = _context.Tiene.Where(t => colmenasIds.Contains(t.ID_Colmena)).ToList();

                apiario.ColmenasInstaladas = apiario.ColmenasInstaladas.Where(t => 
                {
                    var ultimo = historiales.Where(h => h.ID_Colmena == t.ID_Colmena).OrderByDescending(h => h.Fecha_Instalacion).FirstOrDefault();
                    return ultimo != null && ultimo.ID_Apiario == apiario.ID_Apiario;
                }).ToList();
            }

            return View(apiario);
        }

        // GET: Apiarios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Apiarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Apiario apiario)
        {
            if (ModelState.IsValid)
            {
                _context.Apiarios.Add(apiario);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(apiario);
        }
        // GET: Apiarios/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiario = _context.Apiarios.Find(id);
            if (apiario == null)
            {
                return NotFound();
            }

            // Calcular colmenas actuales (activas) para mostrar en la vista
            var todasLasColmenas = _context.Tiene.ToList();
            var colmenasDelApiario = todasLasColmenas.Where(t => t.ID_Apiario == apiario.ID_Apiario).Select(t => t.ID_Colmena).Distinct().ToList();
            
            int colmenasActuales = 0;
            foreach (var colmenaId in colmenasDelApiario)
            {
                var ultimo = todasLasColmenas.Where(h => h.ID_Colmena == colmenaId).OrderByDescending(h => h.Fecha_Instalacion).FirstOrDefault();
                if (ultimo != null && ultimo.ID_Apiario == apiario.ID_Apiario)
                {
                    colmenasActuales++;
                }
            }
            ViewBag.ColmenasActuales = colmenasActuales;

            return View(apiario);
        }

        // POST: Apiarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Apiario apiario)
        {
            if (id != apiario.ID_Apiario)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(apiario);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(apiario);
        }

        // GET: Apiarios/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiario = _context.Apiarios.FirstOrDefault(m => m.ID_Apiario == id);
            if (apiario == null)
            {
                return NotFound();
            }

            return View(apiario);
        }

        // POST: Apiarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var apiario = _context.Apiarios.Find(id);
            if (apiario != null)
            {
                _context.Apiarios.Remove(apiario);
            }
            
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
