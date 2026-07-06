using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace ObligatorioApiario.Models
{
    public class Tiene
    {
        public int ID_Apiario { get; set; }
        [ForeignKey("ID_Apiario")]
        public Apiario? Apiario { get; set; }

        public int ID_Colmena { get; set; }
        [ForeignKey("ID_Colmena")]
        public Colmena? Colmena { get; set; }

        [DataType(DataType.Date)]
        public DateTime Fecha_Instalacion { get; set; }
        public string Sector { get; set; } = string.Empty;
        public string Fila { get; set; } = string.Empty;
        public string Columna { get; set; } = string.Empty;
    }
}
