using System.ComponentModel.DataAnnotations.Schema;

namespace APIMonstre.Models
{
    public class LevelUpQuetes
    {
        public int IdLevelUpQuetes { get; set; }
        public int NiveauDepart { get; set; }
        public int NiveauObjectif { get; set; }
        public string Description { get; set; }
        public bool EstComplete { get; set; } = false;
        [ForeignKey("Personnage")]
        public int PersonnageId { get; set; }
        public Personnage Personnage { get; set; }

        internal void UpdateStatus(int niveau)
        {
            if (NiveauObjectif >= niveau)
            {
                EstComplete = true;
            }
        }
    }
}
