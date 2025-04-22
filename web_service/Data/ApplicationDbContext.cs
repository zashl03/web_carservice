/*
 * Главный класс контекста базы данных приложения.
 * Наследуется от IdentityDbContext для интеграции с ASP.NET Core Identity.
 * Определяет структуру БД, связи между таблицами и настройки сущностей:
 * - Профили клиентов и сотрудников
 * - Автомобили с привязкой к владельцам
 * - Пользовательские расширения Identity
 */
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using web_service.Data.Identity;
using web_service.Data.Entities;

namespace web_service.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // Конструктор с передачей настроек контекста (подключение к БД и др.)
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Таблица клиентских профилей (1-to-1 с пользователями)
        public DbSet<ClientProfile> ClientProfiles { get; set; }

        // Таблица профилей сотрудников (1-to-1 с пользователями)
        public DbSet<EmployeeProfile> EmployeeProfiles { get; set; }

        // Таблица автомобилей (N автомобилей к 1 клиенту)
        public DbSet<CarEntity> Cars { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Инициализация настроек Identity

            // Конфигурация для ClientProfile
            builder.Entity<ClientProfile>(entity =>
            {
                entity.HasKey(c => c.UserId); // Первичный ключ = внешний ключ к пользователю

                // Связь 1-to-1 с ApplicationUser
                entity.HasOne(c => c.User)
                      .WithOne(u => u.ClientProfile)
                      .HasForeignKey<ClientProfile>(c => c.UserId);

                // Связь 1-to-many с автомобилями
                entity.HasMany(c => c.Cars)
                      .WithOne(c => c.Client)
                      .HasForeignKey(c => c.ClientProfileId)
                      .OnDelete(DeleteBehavior.Cascade); // Автоудаление авто при удалении клиента
            });

            // Конфигурация для EmployeeProfile
            builder.Entity<EmployeeProfile>(entity =>
            {
                entity.HasKey(e => e.UserId); // Первичный ключ = ID пользователя

                // Связь 1-to-1 с ApplicationUser
                entity.HasOne(e => e.User)
                      .WithOne(u => u.EmployeeProfile)
                      .HasForeignKey<EmployeeProfile>(e => e.UserId);
            });

            // Конфигурация для CarEntity
            builder.Entity<CarEntity>(entity =>
            {
                entity.HasIndex(c => c.VIN).IsUnique(); // Уникальность VIN на уровне БД
            });
        }
    }
}