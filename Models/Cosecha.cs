using System.ComponentModel.DataAnnotations;
namespace ObligatorioApiario.Models
{
    public class Cosecha
    {
        [Key]
        public int ID_Cosecha { get; set; }
        public string Observaciones { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;

        // Propiedad amigable para la interfaz
        public string Identificador { get; set; } = string.Empty;

        public ICollection<Realiza> RealizadaEn { get; set; } = new List<Realiza>();
        public ICollection<Genera> GeneraBarriles { get; set; } = new List<Genera>();
    }
}
