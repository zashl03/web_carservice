using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using web_service.Data.Identity;
using web_service.Data.Entities;

namespace web_service.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<ClientProfile> ClientProfiles { get; set; }
        public DbSet<EmployeeProfile> EmployeeProfiles { get; set; }
        public DbSet<CarEntity> Cars { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ClientProfile
            builder.Entity<ClientProfile>(entity =>
            {
                entity.HasKey(c => c.UserId);
                entity.HasOne(c => c.User)
                      .WithOne(u => u.ClientProfile)
                      .HasForeignKey<ClientProfile>(c => c.UserId);

                entity.HasMany(c => c.Cars)
                      .WithOne(c => c.Client)
                      .HasForeignKey(c => c.ClientProfileId)
                      .OnDelete(DeleteBehavior.Cascade);  // Удаляем авто при удалении профиля
            });

            // EmployeeProfile
            builder.Entity<EmployeeProfile>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasOne(e => e.User)
                      .WithOne(u => u.EmployeeProfile)
                      .HasForeignKey<EmployeeProfile>(e => e.UserId);
            });

            // CarEntity
            builder.Entity<CarEntity>(entity =>
            {
                entity.HasIndex(c => c.VIN).IsUnique();
            });
        }
    }
}