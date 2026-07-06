using System.ComponentModel.DataAnnotations;
namespace ObligatorioApiario.Models
{
    public class Apicultor
    {
        [Key]
        public int CIApicultor { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        
        public ICollection<ApicultorTelefono> Telefonos { get; set; } = new List<ApicultorTelefono>();
        public ICollection<Visita> Visitas { get; set; } = new List<Visita>();
        public ICollection<Barril> Barriles { get; set; } = new List<Barril>();
    }
}
