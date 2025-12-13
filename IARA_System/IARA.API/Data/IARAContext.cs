using IARA.Domain.Models;
using IARA.Domain.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace IARA.API.Data
{
    public class IARAContext : IdentityDbContext<ApplicationUser, UserRole, string>
    {
        public IARAContext(DbContextOptions<IARAContext> options) : base(options) { }

        // DbSet свойства
        public DbSet<Fisher> Fishers { get; set; }
        public DbSet<Ship> Ships { get; set; }
        public DbSet<License> Licenses { get; set; }
        public DbSet<LogbookEntry> LogbookEntries { get; set; }
        public DbSet<CatchDetail> CatchDetails { get; set; } // ДОБАВЕНО
        public DbSet<AmateurTicket> AmateurTickets { get; set; }
        public DbSet<AmateurCatch> AmateurCatches { get; set; }
        public DbSet<Inspection> Inspections { get; set; }
        public DbSet<Inspector> Inspectors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Конфигурация на връзките
            modelBuilder.Entity<CatchDetail>()
                .HasOne(cd => cd.LogbookEntry)
                .WithMany(le => le.CatchDetails)
                .HasForeignKey(cd => cd.LogbookEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ограничения за данни
            modelBuilder.Entity<CatchDetail>()
                .Property(cd => cd.WeightKgs)
                .HasPrecision(10, 2);

            modelBuilder.Entity<LogbookEntry>()
                .Property(le => le.FuelConsumptionLiters)
                .HasPrecision(10, 2);

            // Индекси за бързи търсения
            modelBuilder.Entity<License>()
                .HasIndex(l => l.ExpiryDate);

            modelBuilder.Entity<AmateurCatch>()
                .HasIndex(ac => ac.CatchDate);

            modelBuilder.Entity<LogbookEntry>()
                .HasIndex(le => le.FishingDate);
        }
    }
}