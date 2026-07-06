using System.ComponentModel.DataAnnotations.Schema;
namespace ObligatorioApiario.Models
{
    public class Genera
    {
        public int ID_Barril { get; set; }
        [ForeignKey("ID_Barril")]
        public Barril? Barril { get; set; }

        public int ID_Cosecha { get; set; }
        [ForeignKey("ID_Cosecha")]
        public Cosecha? Cosecha { get; set; }
    }
}
