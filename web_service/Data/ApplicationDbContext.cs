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
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;

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

        // Таблица записей на обслуживание (N записей к 1 автомобилю)
        public DbSet<RecordEntity> Records { get; set; }
        public DbSet<PartEntity> Parts { get; set; }
        public DbSet<StorageLocationEntity> StorageLocations { get; set; }
        public DbSet<PartInStorageEntity> PartInStorages { get; set; }
        public DbSet<CategoryPartEntity> CategoryParts { get; set; }
        public DbSet<WorkEntity> Works { get; set; } = null!;
        public DbSet<WorkOrderEntity> WorkOrders { get; set; } = null!;
        public DbSet<TypeServiceEntity> TypeServices { get; set; }
        public DbSet<WorkTaskEntity> WorkTasks { get; set; } = null!;
        public DbSet<PartInWorkEntity> PartInWorks { get; set; } = null!;  // ← наша новая сущность
        public DbSet<PaymentEntity> Payments { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Инициализация настроек Identity

            // Конфигурация для ClientProfile
            builder.Entity<ClientProfile>(entity =>
            {
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
                // Связь 1-to-1 с ApplicationUser
                entity.HasOne(e => e.User)
                      .WithOne(u => u.EmployeeProfile)
                      .HasForeignKey<EmployeeProfile>(e => e.UserId);
            });

            // Конфигурация автомобилей: уникальный VIN
            builder.Entity<CarEntity>(entity =>
            {
                entity.HasIndex(car => car.VIN).IsUnique();
            });

            // Конфигурация записей: связь N записей к одному автомобилю
            builder.Entity<RecordEntity>(entity =>
            {
                entity.HasOne(r => r.Car)
                      .WithMany()                  // без коллекции навигации в CarEntity
                      .HasForeignKey(r => r.CarId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            //связь Record -> TypeService
            builder.Entity<RecordEntity>()
                .HasOne(r => r.TypeService)
                .WithMany()         
                .HasForeignKey(r => r.TypeServiceId)
                .OnDelete(DeleteBehavior.Restrict);
            //связь Record -> EmployeeProfile
            builder.Entity<RecordEntity>()
                .HasOne(r => r.Administrator)
                .WithMany()           
                .HasForeignKey(r => r.AdministratorId)
                .OnDelete(DeleteBehavior.SetNull);
            // -------------------------------------------------------------------------------------------------------
            // ЗАПЧАСТИ И СКЛАД
            builder.Entity<PartInStorageEntity>()
                .HasOne(pis => pis.Part) // Односторонняя связь
                .WithMany() // Указываем, что у Part НЕТ обратной навигации
                .HasForeignKey(pis => pis.PartId)
                .OnDelete(DeleteBehavior.Cascade);
            // Аналогично для StorageLocation
            builder.Entity<PartInStorageEntity>()
                .HasOne(pis => pis.StorageLocation)
                .WithMany()
                .HasForeignKey(pis => pis.StorageLocationId);
            // Связь StorageLocation -> EmployeeProfile
            builder.Entity<StorageLocationEntity>()
                .HasOne(sl => sl.Storekeeper)
                .WithMany() // Если нужно двунаправленное отношение, добавьте навигационное свойство в EmployeeProfile
                .HasForeignKey(sl => sl.StorekeeperId)
                .OnDelete(DeleteBehavior.Restrict); // Или другой подходящий вариант

            // Уникальный индекс для PartId + StorageLocationId
            builder.Entity<PartInStorageEntity>()
                .HasIndex(pis => new { pis.PartId, pis.StorageLocationId })
                .IsUnique();

            builder.Entity<StorageLocationEntity>()
                .HasIndex(sl => new { sl.NumberPlace })
                .IsUnique();

            // StorageLocation -> Worker
            builder.Entity<StorageLocationEntity>()
                .HasOne(sl => sl.Storekeeper)
                .WithMany()
                .HasForeignKey(sl => sl.StorekeeperId);

            // Part -> CategoryPart
            builder.Entity<PartEntity>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId);
            // CategoryPart
            builder.Entity<CategoryPartEntity>(entity =>
            {
                // Настройка связи "родитель → дети"
                entity.HasMany(c => c.Children)
                      .WithOne(c => c.ParentCategory)
                      .HasForeignKey(c => c.ParentId)
                      .OnDelete(DeleteBehavior.Restrict); // Или Cascade, если нужно удалять детей при удалении родителя

                // Уникальность названия категории на одном уровне иерархии
                entity.HasIndex(c => new { c.CategoryName, c.ParentId })
                      .IsUnique();
            });

            // WorkOrder -> Record
            builder.Entity<WorkOrderEntity>()
                .HasOne(wo => wo.Record)
                .WithOne()
                .HasForeignKey<WorkOrderEntity>(wo => wo.RecordId);

            // WorkOrder -> WorkTask
            builder.Entity<WorkTaskEntity>()
                .HasOne(wt => wt.WorkOrder)
                .WithMany()
                .HasForeignKey(wt => wt.WorkOrderId);

            // Employee -> WorkTask
            builder.Entity<WorkTaskEntity>()
                .HasOne(wt => wt.Mechanic)
                .WithMany()
                .HasForeignKey(wt => wt.MechanicId);

            // Employee -> WorkTask
            builder.Entity<WorkTaskEntity>()
                .HasOne(wt => wt.Work)
                .WithMany()
                .HasForeignKey(wt => wt.WorkId);

            // WorkTask -> PartInWork 
            builder.Entity<PartInWorkEntity>()
                .HasOne(piw => piw.WorkTask)
                .WithMany()
                .HasForeignKey(piw => piw.WorkTaskId);

            // Employee -> WorkTask
            builder.Entity<PartInWorkEntity>()
                .HasOne(piw => piw.Storekeeper)
                .WithMany()
                .HasForeignKey(piw => piw.StorekeeperId);

            // Part -> WorkTask
            builder.Entity<PartInWorkEntity>()
                .HasOne(piw => piw.Part)
                .WithMany()
                .HasForeignKey(piw => piw.PartId);
        }
    }
}