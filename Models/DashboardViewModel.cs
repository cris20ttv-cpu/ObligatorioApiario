using System.Collections.Generic;

namespace ObligatorioApiario.Models
{
    public class ProduccionMensualData
    {
        public string Mes { get; set; } = string.Empty;
        public decimal TotalKg { get; set; }
    }

    public class ApiarioUbicacionData
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Latitud { get; set; }
        public decimal Longitud { get; set; }
        public string Zona { get; set; } = string.Empty;
    }

    public class DashboardViewModel
    {
        public int TotalApiarios { get; set; }
        public int TotalColmenas { get; set; }
        public decimal TotalKilosMiel { get; set; }
        public decimal EstimadoProximaCosecha { get; set; }
        
        public List<Visita> VisitasPendientes { get; set; } = new List<Visita>();
        public List<ProduccionMensualData> GraficoProduccion { get; set; } = new List<ProduccionMensualData>();
        public List<ApiarioUbicacionData> MapaApiarios { get; set; } = new List<ApiarioUbicacionData>();
    }
}
