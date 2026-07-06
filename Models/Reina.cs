using System.ComponentModel.DataAnnotations;
namespace ObligatorioApiario.Models
{
    public class Reina
    {
        [Key]
        public int ID_Reina { get; set; }

        // Propiedad amigable
        public string Identificador { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;
        public string Nivel_Prod { get; set; } = string.Empty;

        public ICollection<Asigna> ColmenasAsignadas { get; set; } = new List<Asigna>();
    }
}
