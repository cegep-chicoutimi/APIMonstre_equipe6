using APIMonstre.Models;
using Microsoft.EntityFrameworkCore;
using APIMonstre.Data.Context;
using APIMonstre.Migrations;
using APIMonstre.Models;

namespace APIMonstre.Services
{
    public class MonstreMaintenanceService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MonstreMaintenanceService> _logger;

        public MonstreMaintenanceService(IServiceProvider serviceProvider, ILogger<MonstreMaintenanceService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        private async Task ValidateMonsterCount(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            {
                var context = scope.ServiceProvider.GetRequiredService<MonstreContext>();

                //On peut utiliser le context de la même façon que dans nos controllers à partir d'ici
                var monsterCount = await context.InstanceMonstre.CountAsync(cancellationToken);

                if (monsterCount <= 300)
                {
                    //On va creer un nouveau instanceMonstre et l<ajouter a la base de donnes
                    await GenererMonstre(context, monsterCount);
                }
            }
        }

        private async Task GenererMonstre(MonstreContext context, int MonsterCount)
        {

            //Trouver ou le mettre
            //Get tous les cases possible
            List<Tuile> tuilesPossibles = context.Tuile.Where(t => t.EstTraversable == true && (TypeTuile)t.Type != TypeTuile.VILLE).ToList();
            foreach (InstanceMonstre instanceMonstre in context.InstanceMonstre)
            {
                if (tuilesPossibles.FirstOrDefault(t => t.PositionX == instanceMonstre.PositionX && t.PositionY == instanceMonstre.PositionY) != null)
                {
                    tuilesPossibles.Remove(tuilesPossibles.FirstOrDefault(t => t.PositionX == instanceMonstre.PositionX && t.PositionY == instanceMonstre.PositionY));
                }
            }

            //Boucle de génération d'instanceMonstre
            for (int i = MonsterCount; i < 300; i++)
            {
                //Get position du monstre
                Tuile spawnTuile = tuilesPossibles.OrderBy(t => Random.Shared.Next()).First();
                tuilesPossibles.Remove(spawnTuile);

                //Get un type de monstre
                Monster monstre = context.Monster.ToList().OrderBy(m => Random.Shared.Next()).First();

                //Get la ville la plus proche
                List<Tuile> villes = context.Tuile.Where(t => (TypeTuile)t.Type == TypeTuile.VILLE).ToList();
                int distancePP = Math.Abs(villes[0].PositionX - spawnTuile.PositionX) + Math.Abs(villes[0].PositionY - spawnTuile.PositionY);

                foreach (Tuile ville in villes)
                {
                    if (Math.Abs(ville.PositionX - spawnTuile.PositionX) + Math.Abs(ville.PositionY - spawnTuile.PositionY) < distancePP)
                    {
                        distancePP = Math.Abs(ville.PositionX - spawnTuile.PositionX) + Math.Abs(ville.PositionY - spawnTuile.PositionY);
                    }
                }

                //Get le level
                int level = distancePP;

                //Créer l'instance
                context.Add(new InstanceMonstre(spawnTuile, monstre, level));
            }
            await context.SaveChangesAsync();
        }

        //Conçu pour s'exécuter une seule fois et contenir une boucle.
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            TimeSpan _checkInterval = TimeSpan.FromMinutes(30); // Check every 30 minutes
                                                                // Perform initial check on startup
            await ValidateMonsterCount(cancellationToken);

            // Continue checking periodically
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_checkInterval, cancellationToken);
                    await ValidateMonsterCount(cancellationToken);
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
    }
}
