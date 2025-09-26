using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;

namespace APIMonstre.Models
{
    public class InstanceMonstre
    {
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public virtual Tuile Tuile { get; set; }
        public int MonstreId { get; set; }
        public virtual Monster Monstre { get; set; }
        public int Niveau {  get; set; }
        public int PointsVieMax { get; set; }
        public int PointsVieActuels { get; set; }

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
