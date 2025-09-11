using APIMonstre.Models;

namespace APIMonstre.Controllers
{
    public static class TuileGenerator
    {
        private static readonly Random _random = new Random();
        public static Tuile GenerateTuile(int x, int y)
        {
            var type = GetTypeRandom();
            var isTraversable = GetTraversable(type);
            var imageURL = TypeTuileImages[type];

            return new Tuile(x, y, (int)type, isTraversable, imageURL);
        }

        private static bool GetTraversable(TypeTuile type)
        {
            switch (type)
            {
                case TypeTuile.EAU:
                case TypeTuile.MONTAGNE: return false;
                default: return true;
            }
        }

        private static TypeTuile GetTypeRandom()
        {

            var nbRandom = _random.Next(0, 100);

            TypeTuile type;

            if (nbRandom < 20)
            {
                type = TypeTuile.HERBE;
            }
            else if (nbRandom < 30)
            {
                type = TypeTuile.EAU;
            }
            else if (nbRandom < 45)
            {
                type = TypeTuile.MONTAGNE;
            }
            else if (nbRandom < 60)
            {
                type = TypeTuile.FORET;
            }
            else if (nbRandom < 65)
            {
                type = TypeTuile.VILLE;
            }
            else
            {
                type = TypeTuile.ROUTE;
            }

            return type;
        }

        private static readonly Dictionary<TypeTuile, string> TypeTuileImages =
            new Dictionary<TypeTuile, string>
            {
                { TypeTuile.HERBE, "Plains.png" },
                { TypeTuile.EAU, "River.png" },
                { TypeTuile.MONTAGNE, "Mountain.png" },
                { TypeTuile.ROUTE, "Road.png" },
                { TypeTuile.VILLE, "Town.png" },
                { TypeTuile.FORET, "Forest.png" }
            };
    }
}
