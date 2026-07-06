using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace ObligatorioApiario.Models
{
    public class Realiza
    {
        public int ID_Cosecha { get; set; }
        [ForeignKey("ID_Cosecha")]
        public Cosecha? Cosecha { get; set; }

        public int ID_Apiario { get; set; }
        [ForeignKey("ID_Apiario")]
        public Apiario? Apiario { get; set; }

        [DataType(DataType.Date)]
        public DateTime Fecha_Cosecha { get; set; }
        public string Tipo_Miel { get; set; } = string.Empty;
        public int Cant_Barriles { get; set; }
    }
}
