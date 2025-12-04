using APIMonstre.Data.Context;
using APIMonstre.Models;
using APIMonstre.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIMonstre.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PokedexController : ControllerBase
    {
        private readonly MonstreContext _context;

        public PokedexController(MonstreContext context)
        {
            _context = context;
        }

        // POST: api/Pokedex
        [HttpPost]
        public async Task<ActionResult<PokedexResponseDto>> GetPokedex([FromBody] PokedexRequestDto request)
        {
            // Récupère les monstres déjà tués
            var huntedIds = await _context.HuntedMonster
                .Where(pm => pm.IdPersonnage == request.IdPersonnage)
                .Select(pm => pm.IdMonster)
                .ToListAsync();

            // Base query : tous les monstres
            var query = _context.Monster.AsQueryable();

            // Filtre optionnel : type
            if (!string.IsNullOrEmpty(request.TypeMonstre))
            {
                string type = request.TypeMonstre;
                query = query.Where(m => m.Type1 == type || m.Type2 == type);
            }

            // Filtre optionnel : par recherche
            if (!string.IsNullOrEmpty(request.recherche))
            {
                string search = request.recherche.ToLower();
                query = query.Where(m => m.Name.ToLower().Contains(search));
            }

            // ⚠ Important : compte AVANT pagination
            int totalBeforeHuntedFilter = await query.CountAsync();

            // Récupère les monstres filtrés (mais pas paginés)
            var allFilteredMonsters = await query
                .OrderBy(m => m.Name)
                .ToListAsync();

            // Convertit en DTO avec le flag Hunted
            var mapped = allFilteredMonsters
                .Select(m => new HuntedMonsterDto
                {
                    Name = m.Name,
                    SpriteUrl = m.SpriteURL,
                    Hunted = huntedIds.Contains(m.IdMonster)
                })
                .ToList();

            // ➤ Filtre Hunted uniquement maintenant
            if (request.Hunted != null)
            {
                bool filterHunted = request.Hunted.Value;
                mapped = mapped.Where(m => m.Hunted == filterHunted).ToList();
            }

            // Count après le filtre Hunted
            int finalCount = mapped.Count;

            // ➤ Pagination
            int skip = (request.Page - 1) * request.PageSize;

            var paginated = mapped
                .Skip(skip)
                .Take(request.PageSize)
                .ToList();

            // ➤ Réponse finale
            return new PokedexResponseDto
            {
                TotalMonstre = finalCount, // total après tous les filtres
                Page = request.Page,
                PageSize = request.PageSize,
                HuntedMonsters = paginated
            };
        }


    }
}
