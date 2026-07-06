using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ObligatorioApiario.Models
{
    public class Barril
    {
        [Key]
        public int ID_Barril { get; set; }
        public int NumeroFisico { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal Cantidad_Miel { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }
        public string Estado { get; set; } = "En Inventario";
        public int CIApicultor { get; set; }
        [ForeignKey("CIApicultor")]
        public Apicultor? Apicultor { get; set; }
        
        public ICollection<Genera> GeneradoPor { get; set; } = new List<Genera>();
    }
}
