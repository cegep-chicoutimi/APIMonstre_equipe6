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

                var chasseQuete = await _context.ChasseQuetes.FirstOrDefaultAsync(cq => cq.PersonnageId == personnage.IdPersonnage && cq.EstComplete == false);
                var levelUpQuete = await _context.LevelUpQuetes.FirstOrDefaultAsync(lq => lq.PersonnageId == personnage.IdPersonnage && lq.EstComplete == false);
                
                if (dto.Victoire)
                {
                    // recupere des pieces d'or en fonction du niveau du monstre
                    int piecesOrGagnees = tuile.Monstre.Niveau * 3;
                    personnage.PiecesOr += piecesOrGagnees;
                    if (chasseQuete != null)
                    {
                        if (chasseQuete.Type.Equals(tuile.Monstre.Type1) || chasseQuete.Type.Equals(tuile.Monstre.Type2))
                        {
                            // update le nombre de monstre tue et passe EstComplete a true si quete finie
                            dto.ChasseQuetes = await quetesService.UpdateChasseQuete(chasseQuete);
                        }
                    }
                    if (levelUpQuete != null)
                    {
                        if (dto.LevelUp != null)
                        {
                            // update le status de la quete si le niveau du personnage est >= au niveau objectif
                            dto.LevelUpQuetes = await quetesService.UpdateLevelUpQuete(levelUpQuete, dto.LevelUp.Niveau);   
                        }
                    }
                }
                else
                {
                    dto.ChasseQuetes = new (chasseQuete);
                    dto.LevelUpQuetes = new (levelUpQuete);
                }
                var randonneQuete = await _context.RandonneQuetes.FirstOrDefaultAsync(rq => rq.PersonnageId == personnage.IdPersonnage && rq.EstComplete == false);
                if (randonneQuete != null)
                {
                    dto.RandonneQuetes = await quetesService.UpdateRandonneQueteAsync(randonneQuete, dto.PositionX, dto.PositionY);

                    // si la randonnee vient d'être complétée et a une récompense XP, l'appliquer et re-vérifier une LevelUpQuete
                    if (dto.RandonneQuetes.EstComplete && randonneQuete.XpRecompense > 0)
                    {
                        var levelUpFromRando = CombatService.AppliquerExperience(personnage, randonneQuete.XpRecompense);

                        _context.Entry(personnage).State = EntityState.Modified;
                        await _context.SaveChangesAsync();

                        if (levelUpFromRando != null)
                        {
                            dto.LevelUp = levelUpFromRando;

                            // recharger et re-évaluer la LevelUpQuete si elle existe
                            levelUpQuete = await _context.LevelUpQuetes.FirstOrDefaultAsync(lq => lq.PersonnageId == personnage.IdPersonnage && lq.EstComplete == false);
                            if (levelUpQuete != null)
                            {
                                dto.LevelUpQuetes = await quetesService.UpdateLevelUpQuete(levelUpQuete, personnage.Niveau);
                            }
                        }
                    }
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

        [HttpGet]
        [Route("Classement/{ordre}")]
        public async Task<IEnumerable<LigneClassementDto>> ClassementPersonnage(string ordre)
        {
            switch (ordre)
            {
                case "Niveau":
                {
                    var top = await _context.Personnage
                        .OrderByDescending(p => p.Niveau)
                        .ThenByDescending(p => p.Experience)
                        .Take(10)
                        .ToListAsync();

                    return top.Select((p, index) => new LigneClassementDto
                    {
                        Rang = index + 1,
                        Pseudo = p.Nom,
                        Valeur = p.Niveau
                    });
                }

                case "Force":
                {
                    var top = await _context.Personnage
                        .OrderByDescending(p => p.Force)
                        .ThenByDescending(p => p.Niveau)
                        .Take(10)
                        .ToListAsync();

                    return top.Select((p, index) => new LigneClassementDto
                    {
                        Rang = index + 1,
                        Pseudo = p.Nom,
                        Valeur = p.Force
                    });
                }

                case "HuntedMonster":
                {
                    // Compter côté base et récupérer les top 10 idPersonnage
                    var topHunted = await _context.HuntedMonster
                        .GroupBy(h => h.IdPersonnage)
                        .Select(g => new { IdPersonnage = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .Take(10)
                        .ToListAsync();

                    // Récupérer les pseudos correspondants en une requête
                    var personnagesMap = await _context.Personnage
                        .Where(p => topHunted.Select(t => t.IdPersonnage).Contains(p.IdPersonnage))
                        .ToDictionaryAsync(p => p.IdPersonnage, p => p.Nom);

                    return topHunted.Select((x, index) => new LigneClassementDto
                    {
                        Rang = index + 1,
                        Pseudo = personnagesMap.TryGetValue(x.IdPersonnage, out var nom) ? nom : $"#{x.IdPersonnage}",
                        Valeur = x.Count
                    }).ToList();
                }

                default:
                {
                    var top = await _context.Personnage
                        .OrderByDescending(p => p.Niveau)
                        .Take(10)
                        .ToListAsync();

                    return top.Select((p, index) => new LigneClassementDto
                    {
                        Rang = index + 1,
                        Pseudo = p.Nom,
                        Valeur = p.Niveau
                    });
                }
            }
        }

        private void UpdateQuetes(PersonnageInfosCombatDto dto)
        {
            
            throw new NotImplementedException();
        }

        //GET: api/Personnages
        //[HttpGet]
        //[Route("{idUtilisateur}")]
        // public async Task<ActionResult<IEnumerable<Personnage>>> GetPersonnages(int idUtilisateur)
        // {
        //     return await _context.Personnage.Where(p => p.IdUtilisateur == idUtilisateur).ToListAsync();
        // }

        //GET: api/Personnages/5
        [HttpPost]
        public async Task<ActionResult<ProfilResponseDto>> GetPersonnage([FromBody] PersonnageRequestDto request)
        {
            var personnage = await _context.Personnage.FindAsync(request.IdPersonnage);

            if (personnage == null)
            {
                return NotFound();
            }

            var PersonnageDto = new ProfilResponseDto() { 
                Pseudo = personnage.Nom,
                Niveau = personnage.Niveau,
                Experience = personnage.Experience,
                PointsVie = personnage.PointsVie,
                PointsVieMax = personnage.PointsVieMax,
                Force = personnage.Force,
                Defense = personnage.Defense,
                PiecesOr = personnage.PiecesOr,
            };

            return PersonnageDto;
        }

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

        [HttpGet("debug/huntedstats")]
        public async Task<ActionResult> DebugHuntedStats()
        {
            var total = await _context.HuntedMonster.CountAsync();
            var perPlayer = await _context.HuntedMonster
                .GroupBy(h => h.IdPersonnage)
                .Select(g => new { IdPersonnage = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var sample = await _context.HuntedMonster.Take(20).ToListAsync();

            return Ok(new { total, perPlayer, sample });
        }
    }
}
