using System.ComponentModel.DataAnnotations.Schema;
namespace ObligatorioApiario.Models
{
    public class Planifica
    {
        public int ID_Visita { get; set; }
        [ForeignKey("ID_Visita")]
        public Visita? Visita { get; set; }

        public int ID_Herramienta { get; set; }
        [ForeignKey("ID_Herramienta")]
        public Herramienta? Herramienta { get; set; }

        public int Cantidad { get; set; }
    }
}
