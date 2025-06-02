using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_service.Data.Entities
{
    public class RecordEntity
    {
        [Key]
        public Guid Id { get; set; }

        // Статус заявки (например: "New", "Confirmed", "Rejected", "Completed" и т.п.)
        [Required, MaxLength(50)]
        public string Status { get; set; }

        // Дата и время приёма (назначения). Должна быть ≥ сегодня.
        [Required]
        public DateTime DateAppointment { get; set; }

        // Комментарий клиента
        [MaxLength(500)]
        public string? ClientComment { get; set; }

        // Причина отказа (заполняет администратор), может быть пустой
        [MaxLength(500)]
        public string? RejectReason { get; set; }

        // Ссылка на автомобиль (CarEntity)
        [Required]
        public Guid CarId { get; set; }
        [ForeignKey(nameof(CarId))]
        public virtual CarEntity Car { get; set; } = null!;

        // Ссылка на администратора (EmployeeProfile.UserId), необязательно при создании
        public string? AdministratorId { get; set; }
        [ForeignKey(nameof(AdministratorId))]
        public virtual EmployeeProfile? Administrator { get; set; }

        // Ссылка на услугу (одна заявка — одна услуга)
        [Required]
        public Guid TypeServiceId { get; set; }
        [ForeignKey(nameof(TypeServiceId))]
        public virtual TypeServiceEntity TypeService { get; set; } = null!;

        // Дата создания заявки (автоматически заполняется)
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
