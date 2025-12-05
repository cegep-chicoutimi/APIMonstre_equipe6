using APIMonstre.Data.Context;
using APIMonstre.Models;
using Microsoft.EntityFrameworkCore;

namespace APIMonstre.Services
{
    public class QuetesMaintenanceService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MonstreMaintenanceService> _logger;
        private readonly QuetesState _state;

        private const int CHECK_INTERVAL = 10; // minutes
        private const int MIN_MONSTER_TO_KILL = 8;
        private const int MAX_MONSTER_TO_KILL = 15;
        private const int MAX_LEVEL_TO_REACH = 4;
        private static readonly int[] XP_TO_EARN = { 100, 150, 200, 250, 300, 350, 400, 450, 500 };

        public QuetesMaintenanceService(IServiceProvider serviceProvider,
                                        ILogger<MonstreMaintenanceService> logger,
                                        QuetesState state)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _state = state;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            TimeSpan interval = TimeSpan.FromMinutes(CHECK_INTERVAL);

            // Première initialisation du prochain refresh
            _state.NextRefreshUtc = GetNextRefreshTime();

            await ValidatePerTypeQuestCount(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(interval, cancellationToken);

                    // 🔥 Mise à jour du prochain refresh toutes les 10 min
                    _state.NextRefreshUtc = GetNextRefreshTime();
                    Console.WriteLine("refresh");

                    await ValidatePerTypeQuestCount(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur dans le cycle de maintenance des quêtes");
                }
            }
        }

        // 🟩 Calcul simple : prochain refresh = maintenant + 10 minutes
        private DateTime GetNextRefreshTime()
        {
            return DateTime.UtcNow.AddMinutes(CHECK_INTERVAL);
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
            var context = scope.ServiceProvider.GetRequiredService<MonstreContext>();

            var personnages = await GetPersonnages(cancellationToken, context);
            var newChasse = new List<ChasseQuetes>();
            var newLevelUp = new List<LevelUpQuetes>();
            var newRando = new List<RandonneQuetes>();

            foreach (var p in personnages)
            {
                if (p.ChasseQuetes.FirstOrDefault(q => q.EstComplete == false) == null)
                    newChasse.Add(await GenerateChasseQuetes(context, p.IdPersonnage));

                if (p.LevelUpQuetes.FirstOrDefault(q => q.EstComplete == false) == null)
                    newLevelUp.Add(await GenerateLevelUpQuetes(p));

                if (p.RandonneQuetes.FirstOrDefault(q => q.EstComplete == false) == null)
                    newRando.Add(await GenerateRandonneQuetes(context, p));
            }

            await context.AddRangeAsync(newChasse, cancellationToken);
            await context.AddRangeAsync(newLevelUp, cancellationToken);
            await context.AddRangeAsync(newRando, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        private async Task<List<Personnage>> GetPersonnages(CancellationToken cancellationToken, MonstreContext context)
        {
            return await context.Personnage
                .Include(p => p.ChasseQuetes.Where(q => !q.EstComplete))
                .Include(p => p.LevelUpQuetes.Where(q => !q.EstComplete))
                .Include(p => p.RandonneQuetes.Where(q => !q.EstComplete))
                .ToListAsync(cancellationToken);
        }

        private async Task<RandonneQuetes> GenerateRandonneQuetes(MonstreContext context, Personnage personnage)
        {
            Tuile randomTuile;
            do
            {
                randomTuile = await context.Tuile.OrderBy(t => t.PositionX)
                    .Skip(Random.Shared.Next(await context.Tuile.CountAsync()))
                    .FirstAsync();
            } while (randomTuile.Type is (int)TypeTuile.EAU or (int)TypeTuile.MONTAGNE);

            var xp = XP_TO_EARN[Random.Shared.Next(XP_TO_EARN.Length)];

            return new RandonneQuetes
            {
                PersonnageId = personnage.IdPersonnage,
                TuileX = randomTuile.PositionX,
                TuileY = randomTuile.PositionY,
                Description = $"Rejoignez la tuile {randomTuile.PositionX};{randomTuile.PositionY}",
                XpRecompense = xp,
                Nom = $"Randonnée vers ({randomTuile.PositionX},{randomTuile.PositionY})"
            };
        }

        private async Task<LevelUpQuetes> GenerateLevelUpQuetes(Personnage personnage)
        {
            var levelToGain = personnage.Niveau + Random.Shared.Next(1, MAX_LEVEL_TO_REACH);
            var xp = XP_TO_EARN[Random.Shared.Next(XP_TO_EARN.Length)];

            return new LevelUpQuetes
            {
                PersonnageId = personnage.IdPersonnage,
                NiveauDepart = personnage.Niveau,
                NiveauObjectif = levelToGain,
                Description = $"Atteignez le niveau {levelToGain}",
                XpRecompense = xp,
                Nom = $"Atteindre le niveau {levelToGain}"
            };
        }

        private async Task<ChasseQuetes> GenerateChasseQuetes(MonstreContext context, int idPersonnage)
        {
            Monster randomMonster = await context.Monster.ElementAtAsync(Random.Shared.Next(context.Monster.Count()));
            string typeToHunt = randomMonster.Type2 == null
                ? randomMonster.Type1
                : (Random.Shared.Next(1,2) == 1 ? randomMonster.Type1 : randomMonster.Type2);

            var nbToKill = Random.Shared.Next(MIN_MONSTER_TO_KILL, MAX_MONSTER_TO_KILL);
            var xp = XP_TO_EARN[Random.Shared.Next(XP_TO_EARN.Length)];

            return new ChasseQuetes
            {
                PersonnageId = idPersonnage,
                Type = typeToHunt,
                ObjectifTue = nbToKill,
                Description = $"Tuez {nbToKill} monstres de type {typeToHunt}",
                XpRecompense = xp,
                Nom = $"Chasseur de monstres {typeToHunt.ToUpper()}"
            };
        }
    }
}
