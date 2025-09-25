namespace APIMonstre.Models
{
    public class InstanceMonstreDto
    {
        public int Id { get; set; }
        public int MonstreId { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string SpriteUrl { get; set; } = string.Empty;
        public int Niveau { get; set; }

        // Position sur la carte
        public int X { get; set; }
        public int Y { get; set; }

        // Statistiques calculées du monstre
        public int PointsVieActuels { get; set; }
        public int PointsVieMax { get; set; }
        public int Attaque { get; set; }
        public int Defense { get; set; }
        public int Vitesse { get; set; }

        // Informations supplémentaires
        public int ExperienceDonnee { get; set; }
        public bool EstVivant { get; set; }

        public InstanceMonstreDto ConvertirInstanceMonstreVersDto(InstanceMonstre instanceMonstre)
        {
            return new InstanceMonstreDto
            {
                Id = instanceMonstre.Id,
                MonstreId = instanceMonstre.MonstreId,
                Nom = instanceMonstre.Monstre.Name,
                SpriteUrl = instanceMonstre.Monstre.SpriteURL,
                Niveau = instanceMonstre.Niveau,
                X = instanceMonstre.PositionX,
                Y = instanceMonstre.PositionY,
                PointsVieActuels = instanceMonstre.PointsVieActuels,
                PointsVieMax = instanceMonstre.CalculerPointsVieMax(),
                Attaque = instanceMonstre.CalculerAttaque(),
                Defense = instanceMonstre.CalculerDefense(),
                Vitesse = instanceMonstre.CalculerVitesse(),
                ExperienceDonnee = instanceMonstre.Monstre.ExperienceBase * instanceMonstre.Niveau,
                EstVivant = instanceMonstre.PointsVieActuels > 0
            };
        }
    }
}
