using APIMonstre.Data.Context;
using APIMonstre.Models;
using APIMonstre.Models.Dto;

namespace APIMonstre.Services
{
    public static class CombatService
    {
        private static int[] seuilsExperience = new int[]
        {
            550, 650, 800, 950, 1050, 1200, 1350, 1450, 1600, 1750,
            1850, 2000, 2150, 2250, 2400, 2550, 2650, 2800, 2950, 3100,
            3200, 3350, 3500, 3600, 3750, 3900, 4000, 4150, 4300, 4400,
            4550, 4700, 4800, 4950, 5100, 5200, 5350, 5500, 5600, 5750,
            5900, 6000, 6150, 6300, 6400, 6550, 6700, 6800, 6950, 7100
        };

        public static PersonnageInfosCombatDto Combattre(Personnage personnage, TuileAvecInfosDto tuile, MonstreContext context)
        {
            double randomModifier = Random.Shared.NextDouble() * (1.25 - 0.8) + 0.8;
            int degatsMonstre =  (int)(((personnage.Force * 2) - tuile.Monstre.Defense) * randomModifier);

            randomModifier = Random.Shared.NextDouble() * (1.25 - 0.8) + 0.8;
            int degatsPersonnage = (int)((tuile.Monstre.Attaque - (personnage.Defense * 1.5)) * randomModifier);

            if (degatsMonstre < 0) degatsMonstre = 0;
            if (degatsPersonnage < 0) degatsPersonnage = 0;

            if(degatsPersonnage >= personnage.PointsVie)
            {
                personnage.PositionX = personnage.DernierVillageX;
                personnage.PositionY = personnage.DernierVillageY;
                personnage.PointsVie = personnage.PointsVieMax;

                return new PersonnageInfosCombatDto(personnage, false, true, null);
            }
            if (degatsMonstre >= tuile.Monstre.PointsVieActuels)
            {
                context.InstanceMonstre.Remove(context.InstanceMonstre.FirstOrDefault(im => im.PositionX == tuile.Monstre.X && im.PositionY == tuile.Monstre.Y));
                personnage.Experience += tuile.Monstre.ExperienceDonnee;

                personnage.PositionX = tuile.PositionX;
                personnage.PositionY = tuile.PositionY;
                personnage.PointsVie -= degatsPersonnage;

                if (GagnerUnNiveau(personnage))
                {
                    return new PersonnageInfosCombatDto(personnage, true, false, new PersonnageLevelUpDto
                    {
                        Niveau = personnage.Niveau,
                        PointsVieMax = personnage.PointsVieMax,
                        Force = personnage.Force,
                        Defense = personnage.Defense,
                        SeuilsExperienceProchainNiveau = seuilsExperience[personnage.Niveau - 1]
                    });
                }
                
                return new PersonnageInfosCombatDto(personnage, true, false, null);
            }

            InstanceMonstre instance = context.InstanceMonstre.FirstOrDefault(im => im.PositionX == tuile.Monstre.X && im.PositionY == tuile.Monstre.Y);

            personnage.PointsVie -= degatsPersonnage;
            instance.PointsVieActuels -= degatsMonstre;
            
            context.Entry(instance).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            return new PersonnageInfosCombatDto(personnage, false, false, null);
        }
        private static bool GagnerUnNiveau(Personnage personnage)
        {
            if(personnage.Experience >= seuilsExperience[personnage.Niveau - 1])
            {
                personnage.Niveau ++;
                personnage.Force++;
                personnage.Defense++;
                personnage.PointsVieMax ++;
                personnage.PointsVie = personnage.PointsVieMax;
                personnage.Experience = 0;

                return true;
            }

            return false;
        }
    }
}
