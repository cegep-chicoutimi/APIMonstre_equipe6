namespace APIMonstre.Models
{
    public class Monster
    {
        public int IdMonster { get; set; }
        public int PokemonId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PointsVieBase { get; set; } 
        public int ForceBase { get; set; }
        public int DefenseBase { get; set; }
        public int ExperienceBase { get; set; }
        public string SpriteURL { get; set; } = string.Empty;
        public string Type1 { get; set; } = string.Empty;
        public string? Type2 { get; set; } = string.Empty;
    }
}
