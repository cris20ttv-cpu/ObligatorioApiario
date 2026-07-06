using System.ComponentModel.DataAnnotations;
namespace ObligatorioApiario.Models
{
    public class Herramienta
    {
        [Key]
        public int ID_Herramienta { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;

        public ICollection<Planifica> PlanificadasEn { get; set; } = new List<Planifica>();
    }
}
