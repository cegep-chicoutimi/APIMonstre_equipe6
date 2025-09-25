using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;

namespace APIMonstre.Models
{
    public class InstanceMonstre
    {
        public int Id { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public Tuile Tuile { get; set; }
        public int MonstreId { get; set; }
        public Monster Monstre { get; set; }
        public int Niveau {  get; set; }
        public int PointsVieMax { get; set; }
        public int PointsVieActuels { get; set; }

        public InstanceMonstre() { }

        public int CalculerAttaque()
        {
            return 0;
        }
        public int CalculerDefense()
        {
            return 0;
        }
        public int CalculerVitesse()
        {
            return 0;
        }
        public int CalculerPointsVieMax()
        {
            return 0;
        }
    }
}
