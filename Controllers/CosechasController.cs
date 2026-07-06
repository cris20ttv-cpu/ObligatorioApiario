using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObligatorioApiario.Data;
using ObligatorioApiario.Models;

namespace ObligatorioApiario.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class CosechasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CosechasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cosechas/Create?apiarioId=5
        public async Task<IActionResult> Create(int apiarioId)
        {
            var apiario = await _context.Apiarios
                .FirstOrDefaultAsync(a => a.ID_Apiario == apiarioId);

            if (apiario == null)
                return NotFound();

            ViewBag.ApiarioId = apiarioId;
            ViewBag.ApiarioNombre = apiario.Nombre;

            // Cargar colmenas activas para la UI
            var todasLasColmenas = await _context.Tiene.Include(t => t.Colmena).ToListAsync();
            var colmenasDelApiario = todasLasColmenas.Where(t => t.ID_Apiario == apiarioId).Select(t => t.ID_Colmena).Distinct().ToList();
            
            var colmenasActivas = new List<Colmena>();
            foreach (var colmenaId in colmenasDelApiario)
            {
                var ultimo = todasLasColmenas.Where(h => h.ID_Colmena == colmenaId).OrderByDescending(h => h.Fecha_Instalacion).FirstOrDefault();
                if (ultimo != null && ultimo.ID_Apiario == apiarioId)
                {
                    colmenasActivas.Add(ultimo.Colmena);
                }
            }
            ViewBag.ColmenasActivas = colmenasActivas.OrderBy(c => c.Identificador).ToList();

            return View(new Realiza { ID_Apiario = apiarioId, Fecha_Cosecha = DateTime.Now });
        }

        // Clases para recibir el payload JSON de la UI interactiva
        public class AporteDTO
        {
            public int ColmenaId { get; set; }
            public decimal Kilos { get; set; }
            public string TempBarrelId { get; set; }
        }

        public class CosechaPayload
        {
            public int ApiarioId { get; set; }
            public DateTime Fecha { get; set; }
            public string TipoMiel { get; set; }
            public string Estado { get; set; }
            public string Observaciones { get; set; }
            public List<string> Barriles { get; set; } 
            public List<AporteDTO> Aportes { get; set; }
        }

        // POST: Cosechas/CreateJson
        [HttpPost]
        public async Task<IActionResult> CreateJson([FromBody] CosechaPayload payload)
        {
            if (payload == null || payload.Barriles == null || payload.Aportes == null)
            {
                return BadRequest("Datos inválidos.");
            }

            var apicultor = await _context.Apicultores.FirstOrDefaultAsync();
            if (apicultor == null)
            {
                apicultor = new Apicultor { CIApicultor = 1, Nombre = "Matias", Cargo = "Administrador de Apiario" };
                _context.Apicultores.Add(apicultor);
                await _context.SaveChangesAsync();
            }

            // Respetar la fecha local que envió el navegador, pero asegurando el formato UTC para la BD
            payload.Fecha = DateTime.SpecifyKind(payload.Fecha, DateTimeKind.Utc);

            var cosechasExistentes = await _context.Cosechas.ToListAsync();
            int maxNumero = 0;
            
            foreach (var c in cosechasExistentes)
            {
                if (!string.IsNullOrEmpty(c.Identificador) && c.Identificador.StartsWith("COS-"))
                {
                    var numeroString = c.Identificador.Substring(4);
                    if (int.TryParse(numeroString, out int numero) && numero > maxNumero)
                    {
                        maxNumero = numero;
                    }
                }
            }

            var cosecha = new Cosecha
            {
                Observaciones = payload.Observaciones ?? "",
                Estado = payload.Estado ?? "Completada",
                Identificador = $"COS-{maxNumero + 1:000}"
            };
            _context.Cosechas.Add(cosecha);
            await _context.SaveChangesAsync();

            var realiza = new Realiza
            {
                ID_Apiario = payload.ApiarioId,
                ID_Cosecha = cosecha.ID_Cosecha,
                Fecha_Cosecha = payload.Fecha,
                Tipo_Miel = payload.TipoMiel ?? "Miel multifloral",
                Cant_Barriles = payload.Barriles.Count
            };
            _context.Realiza.Add(realiza);
            await _context.SaveChangesAsync();

            // Buscar números físicos actualmente en uso por barriles en el inventario
            var numerosEnUso = await _context.Barriles
                .Where(b => b.Estado == "En Inventario")
                .Select(b => b.NumeroFisico)
                .ToListAsync();

            int proximoNumero = 1;

            // Mapear los IDs temporales a los IDs reales de barriles
            var mapBarriles = new Dictionary<string, int>();

            foreach(var tempId in payload.Barriles)
            {
                while (numerosEnUso.Contains(proximoNumero))
                {
                    proximoNumero++;
                }

                var barril = new Barril
                {
                    NumeroFisico = proximoNumero,
                    Cantidad_Miel = 0, // Se actualizará con los aportes
                    Precio = 0,
                    CIApicultor = apicultor.CIApicultor
                };
                _context.Barriles.Add(barril);
                await _context.SaveChangesAsync();
                
                numerosEnUso.Add(proximoNumero); // Marcar como usado

                var genera = new Genera
                {
                    ID_Barril = barril.ID_Barril,
                    ID_Cosecha = cosecha.ID_Cosecha
                };
                _context.Genera.Add(genera);

                mapBarriles[tempId] = barril.ID_Barril;
            }

            // Procesar los aportes de las colmenas
            var kilosPorBarril = new Dictionary<int, decimal>();
            foreach (var bId in mapBarriles.Values) kilosPorBarril[bId] = 0;

            foreach(var aporteDto in payload.Aportes)
            {
                if (mapBarriles.TryGetValue(aporteDto.TempBarrelId, out int realBarrilId))
                {
                    var aporta = new Aporta
                    {
                        ID_Colmena = aporteDto.ColmenaId,
                        ID_Barril = realBarrilId,
                        ID_Cosecha = cosecha.ID_Cosecha,
                        Kilos = aporteDto.Kilos
                    };
                    _context.Aporta.Add(aporta);
                    kilosPorBarril[realBarrilId] += aporteDto.Kilos;
                }
            }
            await _context.SaveChangesAsync();

            // Actualizar el peso real de los barriles basado en los aportes
            foreach (var kvp in kilosPorBarril)
            {
                var barril = await _context.Barriles.FindAsync(kvp.Key);
                if (barril != null)
                {
                    barril.Cantidad_Miel = kvp.Value;
                    _context.Update(barril);
                }
            }
            await _context.SaveChangesAsync();

            return Json(new { success = true, redirectUrl = Url.Action("Details", "Apiarios", new { id = payload.ApiarioId }) });
        }
    }
}
