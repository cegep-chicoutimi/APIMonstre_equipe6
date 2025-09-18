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
        

        public Personnage() { }

        public Personnage(int idPersonnage, string nom, int niveau, int experience, int pointsVie, int pointsVieMax, 
            int force, int defense, int positionX, int positionY, int idUtilisateur, DateTime dateCreation)
        {
            IdPersonnage = idPersonnage;
            Nom = nom;
            Niveau = niveau;
            Experience = experience;
            PointsVie = pointsVie;
            PointsVieMax = pointsVieMax;
            Force = force;
            Defense = defense;
            PositionX = positionX;
            PositionY = positionY;
            IdUtilisateur = idUtilisateur;
            DateCreation = dateCreation;
        }
    }
}
