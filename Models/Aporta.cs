using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ObligatorioApiario.Models
{
    public class Aporta
    {
        [Key]
        public int ID_Aporte { get; set; }
        
        public int ID_Colmena { get; set; }
        
        public int ID_Barril { get; set; }
        
        public int ID_Cosecha { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Kilos { get; set; }

        [ForeignKey("ID_Colmena")]
        public Colmena? Colmena { get; set; }
        
        [ForeignKey("ID_Barril")]
        public Barril? Barril { get; set; }

        [ForeignKey("ID_Cosecha")]
        public Cosecha? Cosecha { get; set; }
    }
}
