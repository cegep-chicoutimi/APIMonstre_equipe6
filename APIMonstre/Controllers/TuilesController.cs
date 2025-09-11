using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIMonstre.Data.Context;
using APIMonstre.Models;

namespace APIMonstre.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TuilesController : ControllerBase
    {
        private readonly MonstreContext _context;

        public TuilesController(MonstreContext context)
        {
            _context = context;
        }

        // GET: api/Tuiles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tuile>>> GetTuile()
        {
            return await _context.Tuile.ToListAsync();
        }

        // GET: api/Tuiles/5
        [HttpGet("{x}/{y}")]
        public async Task<ActionResult<Tuile>> GetTuile(int x, int y)
        {
            if (x < 0 || x >= 50 || y < 0 || y >= 50)
            {
                return BadRequest("Les coordonnées doivent être comprises entre 0 et 49 (monde 50x50).");
            }

            var tuile = await _context.Tuile.Where(t => t.PositionX == x && t.PositionY == y).FirstOrDefaultAsync();

            if (tuile == null)
            {
                var tuilesList = await GetTuileAdjacentes(x, y);
                tuile = TuileService.GenerateTuile(x, y, tuilesList);

                _context.Tuile.Add(tuile);
                await _context.SaveChangesAsync();
            }

            return tuile;
        }

        private async Task<List<Tuile>> GetTuileAdjacentes(int x, int y)
        {
            var list = await _context.Tuile
                .Where(t =>
                    (t.PositionX == x - 1 && t.PositionY == y) ||
                    (t.PositionX == x + 1 && t.PositionY == y) ||
                    (t.PositionX == x && t.PositionY == y - 1) ||
                    (t.PositionX == x && t.PositionY == y + 1))
                .ToListAsync();

            return list ?? new List<Tuile>();
        }



        // PUT: api/Tuiles/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTuile(int id, Tuile tuile)
        {
            if (id != tuile.PositionX)
            {
                return BadRequest();
            }

            _context.Entry(tuile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TuileExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Tuiles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Tuile>> PostTuile(Tuile tuile)
        {
            _context.Tuile.Add(tuile);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (TuileExists(tuile.PositionX))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetTuile", new { id = tuile.PositionX }, tuile);
        }

        // DELETE: api/Tuiles/5
        /**[HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTuile(int id)
        {
            var tuile = await _context.Tuile.FindAsync(id);
            if (tuile == null)
            {
                return NotFound();
            }

            _context.Tuile.Remove(tuile);
            await _context.SaveChangesAsync();

            return NoContent();
        }**/

        private bool TuileExists(int id)
        {
            return _context.Tuile.Any(e => e.PositionX == id);
        }
    }
}
