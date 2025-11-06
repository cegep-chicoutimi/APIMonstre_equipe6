using System.ComponentModel.DataAnnotations.Schema;

namespace APIMonstre.Models
{
    public class RandonneQuetes
    {
        public int IdRandonneQuetes { get; set; }
        public int TuileId { get; set; }
        [ForeignKey("Tuile")]
        public Tuile Tuile { get; set; }
        public string Description { get; set; }
        public bool EstComplete { get; set; }
        [ForeignKey("Personnage")]
        public int PersonnageId { get; set; }
        public Personnage Personnage { get; set; }
    }
}
