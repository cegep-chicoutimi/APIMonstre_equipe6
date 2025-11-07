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
using APIMonstre.Services;

namespace APIMonstre.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonnagesController : ControllerBase
    {
        private readonly MonstreContext _context;
        private const int GRID_MIN = 0, GRID_MAX = 49;
        private readonly QuetesService quetesService;

        public PersonnagesController(MonstreContext context)
        {
            _context = context;
            quetesService = new QuetesService(context);
        }

        [HttpGet]
        [Route("{idPersonnage}/{direction}")]
        public async Task<ActionResult<PersonnageInfosCombatDto>> DeplacerPersonnage(int idPersonnage, string direction)
        {
            var personnage = await _context.Personnage.FindAsync(idPersonnage);
            if(personnage == null)
            {
                return NotFound();
            }
            int newX = 0, newY = 0;
            switch (direction.ToLower().Trim())
            {
                case "up":
                case "haut":
                    newY--;
                    break;
                case "down":
                case "bas":
                    newY++;
                    break;
                case "left":
                case "gauche":
                    newX--;
                    break;
                case "right":
                case "droite":
                    newX++;
                    break;
                default:
                    return BadRequest();

            }
            
            if (personnage.PositionX + newX < GRID_MIN || personnage.PositionX + newX > GRID_MAX
                || personnage.PositionY + newY < GRID_MIN || personnage.PositionY + newY > GRID_MAX)
            {
                return BadRequest();
            }

            TuileAvecInfosDto tuile = TuileAvecInfosDto.ConvertirTuileVersDto(_context.Tuile.Where(t => t.PositionX == personnage.PositionX + newX && t.PositionY == personnage.PositionY + newY).FirstOrDefault(), _context);
            
            if (!tuile.EstAccessible)
            {
                return BadRequest();
            }

            PersonnageInfosCombatDto dto = null;

            if (tuile.TypeTuile == TypeTuile.VILLE)
            {
                personnage.DernierVillageX = tuile.PositionX;
                personnage.DernierVillageY = tuile.PositionY;
                personnage.PointsVie = personnage.PointsVieMax;
            }
            if (tuile.Monstre != null)
            {
                dto = CombatService.Combattre(personnage, tuile, _context); 
            }
            else
            {
                personnage.PositionX = tuile.PositionX;
                personnage.PositionY = tuile.PositionY;
                dto = new PersonnageInfosCombatDto(personnage, false, false, null);
            }

            _context.Entry(personnage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                if (dto.Victoire)
                {
                    var ChasseQuete = await _context.ChasseQuetes.FirstOrDefaultAsync(cq => cq.PersonnageId == personnage.IdPersonnage);
                    var levelUpQuete = await _context.LevelUpQuetes.FirstOrDefaultAsync(lq => lq.PersonnageId == personnage.IdPersonnage);
                    if (ChasseQuete != null)
                    {
                        if (ChasseQuete.Type.Equals(tuile.Monstre.Type1) || ChasseQuete.Type.Equals(tuile.Monstre.Type2))
                        {
                            // update le nombre de monstre tue et passe EstComplete a true si quete finie

                        }
                    }
                    if (levelUpQuete != null && dto.LevelUp != null)
                    {
                        // update le status de la quete si le niveau du personnage est >= au niveau objectif
                        dto.LevelUpQuetes = quetesService.UpdateLevelUpQuete(levelUpQuete, dto.LevelUp.Niveau).Result;
                    }
                }
                var randonneQuete = await _context.RandonneQuetes.FirstOrDefaultAsync(rq => rq.PersonnageId == personnage.IdPersonnage);
                if (randonneQuete != null)
                {
                    dto.RandonneQuetes = quetesService.UpdateRandonneQueteAsync(randonneQuete, dto.PositionX, dto.PositionY).Result;
                }
                return dto;
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

            
        }

        private void UpdateQuetes(PersonnageInfosCombatDto dto)
        {
            
            throw new NotImplementedException();
        }

        // GET: api/Personnages
        //[HttpGet]
        //[Route("{idUtilisateur}")]
        //public async Task<ActionResult<IEnumerable<Personnage>>> GetPersonnages(int idUtilisateur)
        //{
        //    return await _context.Personnage.Where(p => p.IdUtilisateur == idUtilisateur).ToListAsync();
        //}

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
