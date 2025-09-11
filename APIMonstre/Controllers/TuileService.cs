using APIMonstre.Models;

namespace APIMonstre.Controllers
{
    public static class TuileService
    {
        private static readonly Random _random = new Random();
        public static Tuile GenerateTuile(int x, int y, IList<Tuile> tuilesAdjacentes)
        {
            var type = GetTypeRandom(tuilesAdjacentes);
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

        private static TypeTuile GetTypeRandom(IList<Tuile> adjacentes)
        {
            // Cloner les probas de base
            var probas = new Dictionary<TypeTuile, int>(baseProbabilites);

            // Si au moins une adjacente est de tel type → on booste la proba
            if (adjacentes.Any(t => t.Type == (int)TypeTuile.FORET))
                probas[TypeTuile.FORET] += 10;  // +10% forêt

            if (adjacentes.Any(t => t.Type == (int)TypeTuile.EAU))
                probas[TypeTuile.EAU] += 10;    // +10% eau

            if (adjacentes.Any(t => t.Type == (int)TypeTuile.ROUTE))
                probas[TypeTuile.ROUTE] += 10;  // +10% route

            // Tirage pondéré
            int total = probas.Values.Sum();
            int nbRandom = _random.Next(0, total);

            int cumul = 0;
            foreach (var kvp in probas)
            {
                cumul += kvp.Value;
                if (nbRandom < cumul)
                    return kvp.Key;
            }

            // Sécurité
            return TypeTuile.HERBE;
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
        private static readonly Dictionary<TypeTuile, int> baseProbabilites = new()
        {
            { TypeTuile.HERBE, 20 },
            { TypeTuile.EAU, 10 },
            { TypeTuile.MONTAGNE, 15 },
            { TypeTuile.FORET, 15 },
            { TypeTuile.VILLE, 5 },
            { TypeTuile.ROUTE, 35 }
        };
    }
}
