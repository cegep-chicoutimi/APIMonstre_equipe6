using APIMonstre.Models;
using Microsoft.EntityFrameworkCore;

namespace APIMonstre.Data.Context
{
    public class MonstreContext : DbContext
    {
        public DbSet<Monster> Monster { get; set; }
        public DbSet<Tuile> Tuile { get; set; }

        public MonstreContext(DbContextOptions<MonstreContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Monster>()
                .HasKey(m => m.IdMonster)
                .HasName("PrimaryKey_MonsterId");

            modelBuilder.Entity<Tuile>()
                .HasKey(pk => new { pk.PositionX, pk.PositionY });

            modelBuilder.Entity<Tuile>()
                .Property(t => t.ImageURL)
                .HasColumnType("varchar")
                .HasMaxLength(150);
        }
    }
}
