namespace APIMonstre.Models
{
    public class TypePositionHint
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int IdPersonnage { get; set; }
        public Personnage Personnage { get; set; }
    }
}
