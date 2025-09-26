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
        }
    }
}
