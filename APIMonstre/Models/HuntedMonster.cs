namespace APIMonstre.Models
{
    public class HuntedMonster
    {
        public int IdPersonnage { get; set; }
        public Personnage Personnage { get; set; }

        public int IdMonster { get; set; }
        public Monster Monster { get; set; }
    }
}
