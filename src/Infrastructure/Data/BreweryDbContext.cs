using Microsoft.EntityFrameworkCore;
using BoldareBrewery.Domain.Data.Entities;

namespace BoldareBrewery.Infrastructure.Data
{
    public class BreweryDbContext : DbContext
    {
        public BreweryDbContext(DbContextOptions<BreweryDbContext> options) : base(options)
        {
        }

        public DbSet<BreweryEntity> Breweries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BreweryEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(50);
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.Property(e => e.BreweryType).HasMaxLength(50);
                entity.Property(e => e.Street).HasMaxLength(200);

                // Nullable coordinate properties
                entity.Property(e => e.Latitude)
                    .HasColumnType("REAL")
                    .IsRequired(false);

                entity.Property(e => e.Longitude)
                    .HasColumnType("REAL")
                    .IsRequired(false);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Ignore computed property
                entity.Ignore(e => e.Location);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
