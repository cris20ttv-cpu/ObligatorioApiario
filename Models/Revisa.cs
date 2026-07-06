using System.ComponentModel.DataAnnotations.Schema;
namespace ObligatorioApiario.Models
{
    public class Revisa
    {
        public int ID_Visita { get; set; }
        [ForeignKey("ID_Visita")]
        public Visita? Visita { get; set; }

        public int ID_Apiario { get; set; }
        [ForeignKey("ID_Apiario")]
        public Apiario? Apiario { get; set; }

        public TimeSpan H_Salida { get; set; }
        public TimeSpan H_Llegada { get; set; }
        public string Clima { get; set; } = string.Empty;
    }
}
