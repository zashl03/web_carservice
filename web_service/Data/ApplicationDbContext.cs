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

        // профили и доменные таблицы
        public DbSet<ClientProfile> ClientProfiles { get; set; }
        public DbSet<EmployeeProfile> EmployeeProfiles { get; set; }
        public DbSet<CarEntity> Cars { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // свяжем профили через ключ UserId
            builder.Entity<ClientProfile>()
                   .HasKey(cp => cp.UserId);
            builder.Entity<ClientProfile>()
                   .HasOne(cp => cp.User)
                   .WithOne(u => u.ClientProfile)
                   .HasForeignKey<ClientProfile>(cp => cp.UserId);

            builder.Entity<EmployeeProfile>()
                   .HasKey(ep => ep.UserId);
            builder.Entity<EmployeeProfile>()
                   .HasOne(ep => ep.User)
                   .WithOne(u => u.EmployeeProfile)
                   .HasForeignKey<EmployeeProfile>(ep => ep.UserId);

            // у машины VIN должен быть уникальным
            builder.Entity<CarEntity>()
                   .HasIndex(c => c.VIN)
                   .IsUnique();
        }
    }
}
