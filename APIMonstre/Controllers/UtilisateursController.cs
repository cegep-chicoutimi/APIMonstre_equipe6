using APIMonstre.Data.Context;
using APIMonstre.Models;
using APIMonstre.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace APIMonstre.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilisateursController : ControllerBase
    {
        private readonly MonstreContext _context;

        public UtilisateursController(MonstreContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult<LoginResponseDto>> Register([FromBody] RegisterRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Pseudo))
            {
                return BadRequest("Email, mot de passe et pseudo sont requis.");
            }
            var existingUtilisateur = await _context.Utilisateur.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUtilisateur != null)
            {
                return BadRequest();
            }
            Utilisateur utilisateur = new Utilisateur() { Email = request.Email, Pseudo = request.Pseudo, MotDePasse = request.Password, DateInscription = DateTime.Now, estConnecte = true};
            _context.Add(utilisateur);
            await _context.SaveChangesAsync();
            utilisateur = await _context.Utilisateur.FirstOrDefaultAsync(_ => _.Email == request.Email);

            Personnage personnage = new Personnage(utilisateur.IdUtilisateur);
            _context.Add(personnage);

            var tuilesVille = await _context.Tuile
                .Where(t => (TypeTuile)t.Type == TypeTuile.VILLE)
                .ToListAsync();

            if (tuilesVille.Count > 0)
            {
                // Sélection aléatoire d'une tuile VILLE
                var rnd = new Random();
                var tuileAleatoire = tuilesVille[rnd.Next(tuilesVille.Count)];

                // Assignation de la tuile au personnage (en supposant que Personnage a une propriété IdTuileSpawn ou similaire)
                personnage.PositionX = tuileAleatoire.PositionX;
                personnage.PositionY = tuileAleatoire.PositionY;

                await _context.SaveChangesAsync();
            }

            await _context.SaveChangesAsync();
            var personnageUtilisateur = await _context.Personnage.FirstOrDefaultAsync(p => p.IdUtilisateur == utilisateur.IdUtilisateur);

            PersonnageDto personnageDto = new(personnageUtilisateur);

            return new LoginResponseDto(utilisateur.IdUtilisateur, utilisateur.Email, utilisateur.Pseudo, personnageDto, utilisateur.estConnecte);
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            if(string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Email et mot de passe sont requis.");
            }

            var existingUtilisateur = await _context.Utilisateur.FirstOrDefaultAsync(u => u.Email == request.Email && u.MotDePasse == request.Password);

            if (existingUtilisateur == null)
            {
                return Unauthorized();
            }
            existingUtilisateur.estConnecte = true;
            _context.Entry(existingUtilisateur).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UtilisateurExists(existingUtilisateur.IdUtilisateur))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            var personnage = await _context.Personnage.FirstOrDefaultAsync(p => p.IdUtilisateur == existingUtilisateur.IdUtilisateur);
            PersonnageDto personnageDto = new(personnage);

            return new LoginResponseDto(existingUtilisateur.IdUtilisateur, existingUtilisateur.Email, existingUtilisateur.Pseudo, personnageDto, existingUtilisateur.estConnecte);
        }

        [HttpPost]
        [Route("logout")]
        public async Task<ActionResult> Logout([FromBody] LoginRequestDto request)
        {
            var existingUtilisateur = await _context.Utilisateur.FirstOrDefaultAsync(u => u.Email == request.Email && u.MotDePasse == request.Password);

            if (existingUtilisateur == null)
            {
                return NotFound();
            }
            existingUtilisateur.estConnecte = false;
            _context.Entry(existingUtilisateur).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UtilisateurExists(existingUtilisateur.IdUtilisateur))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok();
        }

        // GET: api/Utilisateurs/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<Utilisateur>> GetUtilisateur(int id)
        //{
        //    var utilisateur = await _context.Utilisateur.FindAsync(id);

        //    if (utilisateur == null)
        //    {
        //        return NotFound();
        //    }

        //    return utilisateur;
        //}

        //// PUT: api/Utilisateurs/5
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutUtilisateur(int id, Utilisateur utilisateur)
        //{
        //    if (id != utilisateur.IdUtilisateur)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(utilisateur).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!UtilisateurExists(id))
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

        //// POST: api/Utilisateurs
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //public async Task<ActionResult<Utilisateur>> PostUtilisateur(Utilisateur utilisateur)
        //{
        //    _context.Utilisateur.Add(utilisateur);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetUtilisateur", new { id = utilisateur.IdUtilisateur }, utilisateur);
        //}

        //// DELETE: api/Utilisateurs/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteUtilisateur(int id)
        //{
        //    var utilisateur = await _context.Utilisateur.FindAsync(id);
        //    if (utilisateur == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Utilisateur.Remove(utilisateur);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        private bool UtilisateurExists(int id)
        {
            return _context.Utilisateur.Any(e => e.IdUtilisateur == id);
        }
    }
}
