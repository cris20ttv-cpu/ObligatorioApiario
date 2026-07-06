using System.ComponentModel.DataAnnotations;
namespace ObligatorioApiario.Models
{
    public class Apiario
    {
        [Key]
        public int ID_Apiario { get; set; }

        // Mantenemos estas amigables
        public string Nombre { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public string? Notas { get; set; }

        public string Zona { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = true)]
        public decimal Latitud { get; set; }

        [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = true)]
        public decimal Longitud { get; set; }
        
        public int Cant_Colmenas { get; set; }

        public ICollection<Tiene> ColmenasInstaladas { get; set; } = new List<Tiene>();
        public ICollection<Realiza> CosechasRealizadas { get; set; } = new List<Realiza>();
        public ICollection<Revisa> VisitasRecibidas { get; set; } = new List<Revisa>();
    }
}
