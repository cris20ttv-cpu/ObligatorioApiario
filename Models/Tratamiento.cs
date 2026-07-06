using System.ComponentModel.DataAnnotations;
namespace ObligatorioApiario.Models
{
    public class Tratamiento
    {
        [Key]
        public int ID_Tratamiento { get; set; }
        
        // Propiedad amigable
        public string Nombre { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime Fecha_Inicio { get; set; }
        public int Cant_Dosis { get; set; }
        public string Observaciones { get; set; } = string.Empty;

        public ICollection<Aplica> Aplicaciones { get; set; } = new List<Aplica>();
    }
}
