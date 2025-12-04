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

        // GET: api/Pokedex
        public async Task<ActionResult<PokedexResponseDto>> GetPokedex([FromBody] PokedexRequestDto request)
        {
            var huntedIds = await _context.HuntedMonster
                .Where(pm => pm.IdPersonnage == request.IdPersonnage)
                .Select(pm => pm.IdMonster)
                .ToListAsync();

            var query = _context.Monster.AsQueryable();

            if (!string.IsNullOrEmpty(request.TypeMonstre))
            {
                string type = request.TypeMonstre;
                query = query.Where(m => m.Type1 == type || m.Type2 == type);
            }

            if (!string.IsNullOrEmpty(request.recherche))
            {
                string search = request.recherche.ToLower();
                query = query.Where(m => m.Name.ToLower().Contains(search));
            }

            // Total avant pagination
            int totalCount = await query.CountAsync();

            // Apply Pagination
            int skip = (request.Page - 1) * request.PageSize;

            var monsters = await query
                .OrderBy(m => m.Name)    // important pour que la pagination soit stable
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync();

            var result = monsters.Select(m => new HuntedMonsterDto
            {
                Name = m.Name,
                SpriteUrl = m.SpriteURL,
                Hunted = huntedIds.Contains(m.IdMonster)
            }).ToList();

            // Retourne avec info de pagination
            return new PokedexResponseDto { 
                TotalMonstre = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                HuntedMonsters = result
            };
        }

    }
}
