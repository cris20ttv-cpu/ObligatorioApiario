using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ObligatorioApiario.Models
{
    public class Visita
    {
        [Key]
        public int ID_Visita { get; set; }
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }
        public string Temporada { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        
        public int CIApicultor { get; set; }
        [ForeignKey("CIApicultor")]
        public Apicultor? Apicultor { get; set; }

        public ICollection<Planifica> HerramientasPlanificadas { get; set; } = new List<Planifica>();
        public ICollection<Revisa> ApiariosRevisados { get; set; } = new List<Revisa>();
        public ICollection<TareaVisita> Tareas { get; set; } = new List<TareaVisita>();
    }
}
