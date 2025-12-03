using APIMonstre.Data.Context;
using APIMonstre.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;

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
                ChasseQuetes = new(chasse),
                LevelUpQuetes = new(levelUp),
                RandonneQuetes = new(rando),
               
                ServerTimeUtc = DateTime.UtcNow,
                NextRefreshUtc = _state.NextRefreshUtc
            };

            return dto;
        }
    }
}
