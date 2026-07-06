using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ObligatorioApiario.Models
{
    public class ApicultorTelefono
    {
        public int CIApicultor { get; set; }
        [ForeignKey("CIApicultor")]
        public Apicultor? Apicultor { get; set; }
        public int Telefono { get; set; }
    }
}
