using APIMonstre.Data.Context;
using APIMonstre.Models;
using Microsoft.EntityFrameworkCore;

namespace APIMonstre.Services
{
    public class QuetesMaintenanceService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MonstreMaintenanceService> _logger;
        private const int CHECK_INTERVAL = 10;
        private const int MIN_MONSTER_TO_KILL = 8;
        private const int MAX_MONSTER_TO_KILL = 15;
        private const int MAX_LEVEL_TO_REACH = 4;

        public QuetesMaintenanceService(IServiceProvider serviceProvider, ILogger<MonstreMaintenanceService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        //Conçu pour s'exécuter une seule fois et contenir une boucle.
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            TimeSpan _checkInterval = TimeSpan.FromMinutes(CHECK_INTERVAL); // Check every 30 minutes
                                                                            // Perform initial check on startup
            await ValidatePerTypeQuestCount(cancellationToken);

            // Continue checking periodically
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_checkInterval, cancellationToken);
                    await ValidatePerTypeQuestCount(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during periodic monster count validation");
                    // Continue the loop - don't let one failure kill the service
                }
            }

        }

        public async Task CompleteRandonneQuete(RandonneQuetes randoQuest, MonstreContext context) 
        { 
            randoQuest.EstComplete = true;

            context.RandonneQuetes.Update(randoQuest); 
            await context.SaveChangesAsync();
        
        }

        private async Task ValidatePerTypeQuestCount(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            {
                var context = scope.ServiceProvider.GetRequiredService<MonstreContext>();
                var personnages = await GetPersonnages(cancellationToken, context);
                var newChasse = new List<ChasseQuetes>();
                var newLevelUp = new List<LevelUpQuetes>();
                var newRando = new List<RandonneQuetes>();

                foreach (var p in personnages)
                {
                    if (p.ChasseQuetes.Count == 0)
                        newChasse.Add(await GenerateChasseQuetes(context, p.IdPersonnage));

                    if (p.LevelUpQuetes.Count == 0)
                        newLevelUp.Add(await GenerateLevelUpQuetes(p));

                    if (p.RandonneQuetes.Count == 0)
                        newRando.Add(await GenerateRandonneQuetes(context, p));
                }

                await context.AddRangeAsync(newChasse, cancellationToken);
                await context.AddRangeAsync(newLevelUp, cancellationToken);
                await context.AddRangeAsync(newRando, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

            }
        }

        private async Task<List<Personnage>> GetPersonnages(CancellationToken cancellationToken, MonstreContext context)
        {
            var personnages = await context.Personnage
                .Include(p => p.ChasseQuetes.Where(q => !q.EstComplete))
                .Include(p => p.LevelUpQuetes.Where(q => !q.EstComplete))
                .Include(p => p.RandonneQuetes.Where(q => !q.EstComplete))
                .ToListAsync(cancellationToken);
            return personnages;
        }

        private async Task<RandonneQuetes> GenerateRandonneQuetes(MonstreContext context, Personnage personnage)
        {
            Tuile randomTuile;
            do
            {
                randomTuile = await context.Tuile.ElementAtAsync(Random.Shared.Next(context.Tuile.Count()));
            } while (randomTuile.Type.Equals(TypeTuile.EAU) ||
                     randomTuile.Type.Equals(TypeTuile.MONTAGNE));
            
            return new RandonneQuetes { 
                PersonnageId = personnage.IdPersonnage,
                TuileX = randomTuile.PositionX,
                TuileY = randomTuile.PositionY,
                Description = $"Rejoignez la tuile {randomTuile.PositionX};{randomTuile.PositionY}"
            };
        }

        private async Task<LevelUpQuetes> GenerateLevelUpQuetes(Personnage personnage)
        {
            var levelToGain = personnage.Niveau + Random.Shared.Next(MAX_LEVEL_TO_REACH);
            return new LevelUpQuetes { 
                PersonnageId = personnage.IdPersonnage,
                NiveauDepart = personnage.Niveau,
                NiveauObjectif = levelToGain,
                Description = $"Atteignez le niveau {levelToGain}"
            };

        }

        private async Task<ChasseQuetes> GenerateChasseQuetes(MonstreContext context, int idPersonnage)
        {
            Monster randomMonster = await context.Monster.ElementAtAsync(Random.Shared.Next(context.Monster.Count()));
            string typeToHunt;
            if (randomMonster.Type2 == null) {
                typeToHunt = randomMonster.Type1;
            }else
            {
                typeToHunt = Random.Shared.Next(2) == 1 ? randomMonster.Type1 : randomMonster.Type2;
            }
             
            var nbToKill = Random.Shared.Next(MIN_MONSTER_TO_KILL, MAX_MONSTER_TO_KILL);

            return new ChasseQuetes{
                 PersonnageId = idPersonnage,
                 Type = typeToHunt,
                 ObjectifTue = nbToKill,
                 Description = $"Tuez {nbToKill} monstres de type {typeToHunt}"
            };
        }
    }
}
