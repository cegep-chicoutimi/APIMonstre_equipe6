using APIMonstre.Data.Context;
using APIMonstre.Models;
using APIMonstre.Models.Dto;
using APIMonstre.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using System.Threading.Tasks;

namespace APIMonstre.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuetesController : ControllerBase
    {
        private readonly MonstreContext _context;
        private readonly QuetesState _state;

        public QuetesController(MonstreContext context, QuetesState state)
        {
            _context = context;
            _state = state;
        }

        [HttpGet("{idPersonnage}")]
        public async Task<ActionResult<QuetesListDto>> GetChasseQuetes(int idPersonnage)
        {
            var personnage = await _context.Personnage.FindAsync(idPersonnage);

            var chasse = await _context.ChasseQuetes
                .Where(cq => cq.PersonnageId == idPersonnage && !cq.EstComplete)
                .FirstOrDefaultAsync();

            var levelUp = await _context.LevelUpQuetes
                .Where(lq => lq.PersonnageId == idPersonnage && !lq.EstComplete)
                .FirstOrDefaultAsync();

            var rando = await _context.RandonneQuetes
                .Where(rq => rq.PersonnageId == idPersonnage && !rq.EstComplete)
                .FirstOrDefaultAsync();

            var dto = new QuetesListDto
            {
                ChasseQuetes = chasse == null? null : new(chasse),
                LevelUpQuetes = levelUp == null ? null : new(levelUp),
                RandonneQuetes = rando == null ? null : new(rando),
               
                ServerTimeUtc = DateTime.UtcNow,
                NextRefreshUtc = _state.NextRefreshUtc
            };

            return dto;
        }

        [Route("hint")]
        [HttpPost()]
        public async Task<ActionResult<HintResponseDto>> GetChasseQuetes([FromBody] HintRequestDto request)
        {
            var personnage = await _context.Personnage.FindAsync(request.IdPersonnage);
            var posX = personnage.PositionX;
            var posY = personnage.PositionY;
            var typeMonstre = request.TypeMonstre;

            if (personnage.PiecesOr < 100)
            {
                return BadRequest("Tu n'a pas assez de pièces d'or pour obtenir un indice.");
            }

            var monstreProche = GetMonstrePlusProche(posX, posY, typeMonstre);
            if (monstreProche == null)
            {
                return NotFound("Aucun monstre de ce type n'a été trouvé proche du personnage.");
            }

            // prix du hint deduit
            personnage.PiecesOr -= 100;

            var hint = new TypePositionHint {
                PositionX = monstreProche.Result.PositionX,
                PositionY = monstreProche.Result.PositionY,
                Type = typeMonstre,
                IdPersonnage = personnage.IdPersonnage,
                CreatedAt = DateTime.UtcNow
            };
            await _context.TypePositionHint.AddAsync(hint);
            await _context.SaveChangesAsync();

            var historyHints = await _context.TypePositionHint
                .Where(tph => tph.IdPersonnage == request.IdPersonnage)
                .OrderByDescending(tph => tph.CreatedAt)
                .Take(3)
                .ToListAsync();

            var dto = new HintResponseDto
            {
                CurrentHint = new HintDto(hint),
                PiecesOr = personnage.PiecesOr,
                HistoryHints = historyHints
                .Select(tph => new HintDto(tph))
                .ToList()
            };

            return dto;
        }

        private async Task<InstanceMonstre?> GetMonstrePlusProche(int posX, int posY, string typeMonstre)
        {
            return await _context.InstanceMonstre
        .Where(im =>
            im.Monstre.Type1.ToUpper() == typeMonstre.ToUpper() ||
            im.Monstre.Type2.ToUpper() == typeMonstre.ToUpper())
        .OrderBy(im =>
            Math.Abs(im.PositionX - posX) +
            Math.Abs(im.PositionY - posY))
        .FirstOrDefaultAsync();
        }
    }
    
}
