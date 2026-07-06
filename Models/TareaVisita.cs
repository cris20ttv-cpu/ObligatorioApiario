using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ObligatorioApiario.Models
{
    public class TareaVisita
    {
        [Key]
        public int ID_Tarea { get; set; }

        public int ID_Visita { get; set; }
        [ForeignKey("ID_Visita")]
        public Visita? Visita { get; set; }

        public int ID_Colmena { get; set; }
        [ForeignKey("ID_Colmena")]
        public Colmena? Colmena { get; set; }

        public string Descripcion { get; set; } = string.Empty;
        
        public string Estado { get; set; } = "Pendiente";
    }
}
