using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIMonstre.Data.Context;
using APIMonstre.Models;
using APIMonstre.Models.Dto;

namespace APIMonstre.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonnagesController : ControllerBase
    {
        private readonly MonstreContext _context;

        public PersonnagesController(MonstreContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("{idPersonnage}/{direction}")]
        public async Task<ActionResult<int[]>> DeplacerPersonnage(int idPersonnage, string direction)
        {
            var personnage = await _context.Personnage.FindAsync(idPersonnage);
            if(personnage == null)
            {
                return NotFound();
            }
            TuileAvecInfosDto tuile;
            switch (direction)
            {
                case "up":
                    if(personnage.PositionX - 1 < 0)
                    {
                        return BadRequest();
                    }
                    tuile = new TuilesController(_context).GetTuile(personnage.PositionX - 1, personnage.PositionY).Result.Value;
                    if (!tuile.EstAccessible)
                    {
                        return BadRequest();
                    }

                    personnage.PositionX -= 1 ;
                    break;

                case "down":
                    if(personnage.PositionX + 1 > 49)
                    {
                        return BadRequest();
                    }
                    tuile = new TuilesController(_context).GetTuile(personnage.PositionX + 1, personnage.PositionY).Result.Value;
                    if (!tuile.EstAccessible)
                    {
                        return BadRequest();
                    }
                    personnage.PositionX += 1 ;
                    break;

                case "left":
                    if(personnage.PositionY - 1 < 0)
                    {
                        return BadRequest();
                    }
                    tuile = new TuilesController(_context).GetTuile(personnage.PositionX, personnage.PositionY - 1).Result.Value;
                    if (!tuile.EstAccessible)
                    {
                        return BadRequest();
                    }
                    personnage.PositionY -= 1 ;
                    break;

                case "right":
                    if(personnage.PositionY + 1 > 49)
                    {
                        return BadRequest();
                    }
                    tuile = new TuilesController(_context).GetTuile(personnage.PositionX, personnage.PositionY + 1).Result.Value;
                    if (!tuile.EstAccessible)
                    {
                        return BadRequest();
                    }
                    personnage.PositionY += 1 ;
                    
                    break;
                default:
                    return BadRequest();

            }
            _context.Entry(personnage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonnageExists(idPersonnage))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            
            return new int[] { personnage.PositionX, personnage.PositionY};
        }

        // GET: api/Personnages
        [HttpGet]
        [Route("{idUtilisateur}")]
        public async Task<ActionResult<IEnumerable<Personnage>>> GetPersonnages(int idUtilisateur)
        {
            return await _context.Personnage.Where(p => p.IdUtilisateur == idUtilisateur).ToListAsync();
        }

        // GET: api/Personnages/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<Personnage>> GetPersonnage(int id)
        //{
        //    var personnage = await _context.Personnage.FindAsync(id);

        //    if (personnage == null)
        //    {
        //        return NotFound();
        //    }

        //    return personnage;
        //}

        // PUT: api/Personnages/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutPersonnage(int id, Personnage personnage)
        //{
        //    if (id != personnage.IdPersonnage)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(personnage).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!PersonnageExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: api/Personnages
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //public async Task<ActionResult<Personnage>> PostPersonnage(Personnage personnage)
        //{
        //    _context.Personnage.Add(personnage);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetPersonnage", new { id = personnage.IdPersonnage }, personnage);
        //}

        // DELETE: api/Personnages/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeletePersonnage(int id)
        //{
        //    var personnage = await _context.Personnage.FindAsync(id);
        //    if (personnage == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Personnage.Remove(personnage);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        private bool PersonnageExists(int id)
        {
            return _context.Personnage.Any(e => e.IdPersonnage == id);
        }
    }
}
