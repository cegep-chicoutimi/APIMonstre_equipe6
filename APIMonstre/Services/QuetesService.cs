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

        public async Task<ChasseQuetes> UpdateChasseQuete(ChasseQuetes chasseQuete)
        {
            chasseQuete.NbTue += 1;
            chasseQuete.UpdateStatus();

            _context.Update(chasseQuete);
            await _context.SaveChangesAsync();

            return chasseQuete;
        }

        public async Task<LevelUpQuetes> UpdateLevelUpQuete(LevelUpQuetes levelUpQuete, int niveau)
        {
            levelUpQuete.UpdateStatus(niveau);

            if (levelUpQuete.EstComplete)
            {
                _context.Update(levelUpQuete);
                await _context.SaveChangesAsync();
            }

            return levelUpQuete;
        }

        public async Task<RandonneQuetes> UpdateRandonneQueteAsync(RandonneQuetes randonneQuete, int positionX, int positionY)
        {
            randonneQuete.UpdateStatus(positionX, positionY);
            if (randonneQuete.EstComplete) 
            {
                _context.Update(randonneQuete);
                await _context.SaveChangesAsync();
            }

            return randonneQuete;
        }
    }
}
