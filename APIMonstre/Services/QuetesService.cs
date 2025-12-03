using APIMonstre.Data.Context;
using APIMonstre.Models;
using System.Threading.Tasks;

namespace APIMonstre.Services
{
    public class QuetesService
    {
        private readonly MonstreContext _context;

        public QuetesService(MonstreContext context)
        {
            _context = context;
        }

        public async Task<ChasseQuetesDto> UpdateChasseQuete(ChasseQuetes chasseQuete)
        {
            chasseQuete.NbTue += 1;
            chasseQuete.UpdateStatus();

            _context.Update(chasseQuete);
            await _context.SaveChangesAsync();

            return new(chasseQuete) { 
                MessageFin = chasseQuete.EstComplete ? "Félicitations ! Vous avez terminé la quête de chasse." : null
            };
        }

        public async Task<LevelUpQuetesDto> UpdateLevelUpQuete(LevelUpQuetes levelUpQuete, int niveau)
        {
            levelUpQuete.UpdateStatus(niveau);

            if (levelUpQuete.EstComplete)
            {
                _context.Update(levelUpQuete);
                await _context.SaveChangesAsync();
            }

            return new(levelUpQuete) { 
                MessageFin = levelUpQuete.EstComplete ? "Félicitations ! Vous avez atteint le niveau objectif de la quête." : null
            };
        }

        public async Task<RandonneQuetesDto> UpdateRandonneQueteAsync(RandonneQuetes randonneQuete, int positionX, int positionY)
        {
            randonneQuete.UpdateStatus(positionX, positionY);
            if (randonneQuete.EstComplete) 
            {
                _context.Update(randonneQuete);
                await _context.SaveChangesAsync();
            }

            return new(randonneQuete) { 
                MessageFin = randonneQuete.EstComplete ? "Félicitations ! Vous avez terminé la quête de randonnée." : null
            };
        }
    }
}
