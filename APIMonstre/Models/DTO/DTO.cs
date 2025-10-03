using APIMonstre.Data.Context;

namespace APIMonstre.Models.Dto
{
    public class LoginRequestDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class RegisterRequestDto
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public string Pseudo { get; set; }
    }
    public class InstanceMonstreDto
    {
        public int MonstreId { get; set; }
        public string Nom { get; set; }
        public string SpriteUrl { get; set; }
        public int Niveau { get; set; }

        // Position sur la carte
        public int X { get; set; }
        public int Y { get; set; }

        // Statistiques calculées du monstre
        public int PointsVieActuels { get; set; }
        public int PointsVieMax { get; set; }
        public int Attaque { get; set; }
        public int Defense { get; set; }

        // Informations supplémentaires
        public int ExperienceDonnee { get; set; }
        public bool EstVivant { get; set; }
    }

    public class TuileAvecInfosDto
    {
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public TypeTuile TypeTuile { get; set; }
        public string ImageURL { get; set; }

        // Monstre présent sur la tuile (null si aucun)
        public InstanceMonstreDto Monstre { get; set; }

        // Indique si le joueur peut se déplacer sur cette tuile
        public bool EstAccessible { get; set; }

        public static InstanceMonstreDto ConvertirInstanceMonstreVersDto(InstanceMonstre instanceMonstre, MonstreContext context)
        {
            instanceMonstre.Monstre = context.Monster.FirstOrDefault(m => m.IdMonster == instanceMonstre.MonstreId);

            return new InstanceMonstreDto
            {
                MonstreId = instanceMonstre.MonstreId,
                Nom = instanceMonstre.Monstre.Name,
                SpriteUrl = instanceMonstre.Monstre.SpriteURL,
                Niveau = instanceMonstre.Niveau,
                X = instanceMonstre.PositionX,
                Y = instanceMonstre.PositionY,
                PointsVieActuels = instanceMonstre.PointsVieActuels,
                PointsVieMax = instanceMonstre.PointsVieMax,
                Attaque = instanceMonstre.CalculerAttaque(),
                Defense = instanceMonstre.CalculerDefense(),
                ExperienceDonnee = instanceMonstre.Monstre.ExperienceBase * instanceMonstre.Niveau,
                EstVivant = instanceMonstre.PointsVieActuels > 0
            };
        }

        public static TuileAvecInfosDto ConvertirTuileVersDto(Tuile tuile, MonstreContext context)
        {
            InstanceMonstre? monstre = context.InstanceMonstre.FirstOrDefault(im => im.PositionX == tuile.PositionX && im.PositionY == tuile.PositionY);

            return new TuileAvecInfosDto
            {
                PositionX = tuile.PositionX,
                PositionY = tuile.PositionY,
                ImageURL = tuile.ImageURL,
                TypeTuile = (TypeTuile)tuile.Type,
                EstAccessible = tuile.EstTraversable,
                Monstre = monstre != null ? ConvertirInstanceMonstreVersDto(monstre, context) : null
            };
        }
    }
}
