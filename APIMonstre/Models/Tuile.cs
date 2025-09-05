namespace APIMonstre.Models
{
    public class Tuile()
    {
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int Type { get; set; }
        public bool EstTraversable { get; set; }
        public string ImageURL { get; set; }

    }
}
