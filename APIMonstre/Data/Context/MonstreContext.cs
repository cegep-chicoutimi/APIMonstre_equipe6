using APIMonstre.Models;
using Microsoft.EntityFrameworkCore;

namespace APIMonstre.Data.Context
{
    public class MonstreContext : DbContext
    {
        public DbSet<Monster> Monster { get; set; }
        public DbSet<Tuile> Tuile { get; set; }
        public DbSet<Utilisateur> Utilisateur { get; set; }
        public DbSet<Personnage> Personnage { get; set; }
        public DbSet<InstanceMonstre> InstanceMonstre {  get; set; }
        public DbSet<ChasseQuetes> ChasseQuetes {  get; set; }
        public DbSet<LevelUpQuetes> LevelUpQuetes {  get; set; }
        public DbSet<RandonneQuetes> RandonneQuetes {  get; set; }
        public DbSet<TypePositionHint> TypePositionHint { get; set; }
        public DbSet<HuntedMonster> HuntedMonster { get; set; }

        public MonstreContext(DbContextOptions<MonstreContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Monster>()
                .HasKey(m => m.IdMonster)
                .HasName("PrimaryKey_MonsterId");
            modelBuilder.Entity<Utilisateur>().HasKey(u => u.IdUtilisateur).HasName("PrimaryKey_UtilisateurId");

            modelBuilder.Entity<Personnage>().HasKey(p => p.IdPersonnage).HasName("PrimaryKey_PersonnageId");

            modelBuilder.Entity<ChasseQuetes>().HasKey(q => q.IdChasseQuetes).HasName("PrimaryKey_ChasseQuetes");

            modelBuilder.Entity<RandonneQuetes>().HasKey(q => q.IdRandonneQuetes).HasName("PrimaryKey_RandonneQuetes");

            modelBuilder.Entity<LevelUpQuetes>().HasKey(q => q.IdLevelUpQuetes).HasName("PrimaryKey_LevelUpQuetes");

            modelBuilder.Entity<TypePositionHint>().HasKey(tph => tph.Id).HasName("PrimaryKey_TypePositionHint");

            modelBuilder.Entity<Tuile>()
                .HasKey(pk => new { pk.PositionX, pk.PositionY });

            modelBuilder.Entity<Tuile>()
                .Property(t => t.ImageURL)
                .HasColumnType("varchar")
                .HasMaxLength(150);

            modelBuilder.Entity<InstanceMonstre>()
               .HasKey(im => new { im.PositionX, im.PositionY });

            // Configure the relationship between InstanceMonstre and Tuile
            modelBuilder.Entity<InstanceMonstre>()
                .HasOne(im => im.Tuile)
                .WithMany() // or WithOne() if it's one-to-one
                .HasForeignKey(im => new { im.PositionX, im.PositionY })
                .HasPrincipalKey(t => new { t.PositionX, t.PositionY });

            // Configure the relationship between InstanceMonstre and Monster
            modelBuilder.Entity<InstanceMonstre>()
                .HasOne(im => im.Monstre)
                .WithMany()
                .HasForeignKey("MonstreId"); // Assuming you'll add a MonstreId property

            modelBuilder.Entity<RandonneQuetes>()
                .HasOne(rq => rq.Tuile)
                .WithMany()
                .HasForeignKey(rq => new {rq.TuileX, rq.TuileY })
                .HasPrincipalKey(t => new { t.PositionX, t.PositionY });

            modelBuilder.Entity<HuntedMonster>()
                .HasKey(pm => new { pm.IdPersonnage, pm.IdMonster }); // clé composite

            modelBuilder.Entity<HuntedMonster>()
                .HasOne(pm => pm.Personnage)
                .WithMany(p => p.HuntedMonster)
                .HasForeignKey(pm => pm.IdPersonnage);

            modelBuilder.Entity<HuntedMonster>()
                .HasOne(pm => pm.Monster)
                .WithMany(m => m.PersonnageTueurs)
                .HasForeignKey(pm => pm.IdMonster);

            modelBuilder.Entity<TypePositionHint>()
               .HasOne(tph => tph.Personnage)
               .WithMany(p => p.TypePositionHints)
               .HasForeignKey(tph => tph.IdPersonnage);
        }
    }
}
