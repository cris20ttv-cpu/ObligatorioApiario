using System.ComponentModel.DataAnnotations.Schema;
namespace ObligatorioApiario.Models
{
    public class Aplica
    {
        public int ID_Colmena { get; set; }
        [ForeignKey("ID_Colmena")]
        public Colmena? Colmena { get; set; }

        public int ID_Tratamiento { get; set; }
        [ForeignKey("ID_Tratamiento")]
        public Tratamiento? Tratamiento { get; set; }
    }
}
