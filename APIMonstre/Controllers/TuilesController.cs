using APIMonstre.Data.Context;
using APIMonstre.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using APIMonstre.Models.Dto;

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

        [HttpPost]
        [Route("explorer")]
        public async Task<ActionResult<TuileAvecInfosDto[]>> GetTuiles([FromBody] int[][] coords)
        {
            var tuiles = new List<Tuile>();
            var tuilesDto = new List<TuileAvecInfosDto>();
            if(coords.Length > 9)
            {
                return Forbid();
            }
            if (!AllCoordsAdjacent(coords))
            {
                return BadRequest();
            }

            for(int i = 0; i < coords.Length; i++)
            {
                tuiles.AddRange(await _context.Tuile.Where(t => t.PositionX == coords[i][0] && t.PositionY == coords[i][1]).ToListAsync());
            }
            foreach(Tuile tuile in tuiles)
            {
                tuilesDto.Add(TuileAvecInfosDto.ConvertirTuileVersDto(tuile, _context));
            }

            return tuilesDto.ToArray();
        }

        //Comprends rien
        private bool AllCoordsAdjacent(int[][] coords)
        {
            var set = new HashSet<(int x, int y)>(coords.Select(c => (c[0], c[1])));
            var visited = new HashSet<(int x, int y)>();
            var stack = new Stack<(int x, int y)>();
            stack.Push((coords[0][0], coords[0][1]));
            visited.Add((coords[0][0], coords[0][1]));

            // Directions possibles (8 directions : N, NE, E, SE, S, SW, W, NW)
            int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1 };

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                for (int d = 0; d < 8; d++)
                {
                    var neighbor = (current.x + dx[d], current.y + dy[d]);
                    if (set.Contains(neighbor) && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        stack.Push(neighbor);
                    }
                }
            }

            // Si toutes les cases sont visitées, elles sont connectées en “L” ou autre
            return visited.Count == set.Count;
        }

        // GET: api/Tuiles/5
        [HttpGet("{x}/{y}")]
        public async Task<ActionResult<TuileAvecInfosDto>> GetTuile(int x, int y)
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

            return TuileAvecInfosDto.ConvertirTuileVersDto(tuile, _context);
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
