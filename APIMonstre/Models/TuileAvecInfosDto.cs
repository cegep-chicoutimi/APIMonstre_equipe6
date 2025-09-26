namespace APIMonstre.Models
{
    public class TuileAvecInfosDto
    {
        public int PositionX {  get; set; }
        public int PositionY { get; set; }
        public int Type { get; set; }
        public bool EstTraversable { get; set; }
        public bool EstVille {  get; set; }
        public Monster Monstre { get; set; }


        public TuileAvecInfosDto ConvertirTuileVersDto(Tuile tuile, Monster monstre)
        {
            return new TuileAvecInfosDto
            {
                PositionX = tuile.PositionX,
                PositionY = tuile.PositionY,
                Type = tuile.Type,
                EstTraversable = tuile.EstTraversable,
                //EstVille = tuile.EstVille,
                //Monstre = monstre != null ? ConvertirInstanceMonstreVersDto(monstre) : null
            };
        }
    }
}
