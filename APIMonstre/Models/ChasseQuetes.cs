using System.ComponentModel.DataAnnotations.Schema;

namespace APIMonstre.Models
{
    public class ChasseQuetes
    {
        public int IdChasseQuetes { get; set; }
        public string Nom { get; set; }
        public string Type { get; set; }
        public int NbTue { get; set; } = 0;
        public int ObjectifTue { get; set; }
        public string Description { get; set; }
        public bool EstComplete { get; set; } = false;
        public int XpRecompense { get; set; }
        public DateTime DateCreation { get; set; } = DateTime.Now;
        [ForeignKey("Personnage")]
        public int PersonnageId { get; set; }
        public Personnage Personnage { get; set; }

        public void UpdateStatus() 
        {
            if (NbTue == ObjectifTue) 
            {
                EstComplete = true;    
            }

        }
    }
}
