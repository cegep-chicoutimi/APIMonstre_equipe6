using System.ComponentModel.DataAnnotations.Schema;

namespace APIMonstre.Models
{
    public class Personnage
    {
        public int IdPersonnage { get; set; }
        public string Nom { get; set; } = string.Empty;
        public int Niveau { get; set; }
        public int Experience { get; set; }
        public int PointsVie { get; set; }
        public int PointsVieMax { get; set;}
        public int Force { get; set; }
        public int Defense { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public DateTime DateCreation { get; set; }
        [ForeignKey("Utilisateur")]
        public int IdUtilisateur { get; set; }
        public Utilisateur Utilisateur { get; set; }
        


        public Personnage(int idUtilisateur)
        {
            Niveau = 1;
            Experience = 0;
            PointsVie = 100;
            PointsVieMax = 100;
            Force = new Random().Next(1, 11);
            Defense = new Random().Next(1, 11);
            PositionX = new Random().Next(2, 48);
            PositionY = new Random().Next(2, 48);
            DateCreation = DateTime.Now;
            IdUtilisateur = idUtilisateur;
        }
    }
}
