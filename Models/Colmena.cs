using System.ComponentModel.DataAnnotations;
namespace ObligatorioApiario.Models
{
    public class Colmena
    {
        [Key]
        public int ID_Colmena { get; set; }

        // Propiedad amigable
        public string Identificador { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;
        public int Cant_Bastidores { get; set; }
        public int Cant_Abejas_Estimada { get; set; }
        public string Fortaleza_Abejas { get; set; } = string.Empty;

        public ICollection<Tiene> Instalaciones { get; set; } = new List<Tiene>();
        public ICollection<Asigna> ReinasAsignadas { get; set; } = new List<Asigna>();
        public ICollection<Aplica> TratamientosAplicados { get; set; } = new List<Aplica>();
    }
}
