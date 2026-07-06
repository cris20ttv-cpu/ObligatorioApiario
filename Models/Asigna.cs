using System.ComponentModel.DataAnnotations.Schema;
namespace ObligatorioApiario.Models
{
    public class Asigna
    {
        public int ID_Colmena { get; set; }
        [ForeignKey("ID_Colmena")]
        public Colmena? Colmena { get; set; }

        public int ID_Reina { get; set; }
        [ForeignKey("ID_Reina")]
        public Reina? Reina { get; set; }

        public DateTime Fecha_Asignacion { get; set; }
    }
}
