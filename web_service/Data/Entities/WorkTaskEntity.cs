using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_service.Data.Entities
{
    public class WorkTaskEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше 0")]
        public int Quantity { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = null!; // например, "New", "InProgress", "Completed"

        [Required, MaxLength(20)]
        public string MeasureUnit { get; set; } = null!; // ед. измерения: “шт”, “час” и т. п.

        [Column(TypeName = "numeric(10,2)")]
        public decimal? FactCost { get; set; } // фактическая стоимость выполненной работы

        // Ссылка на механика (EmployeeProfile)
        public string? MechanicId { get; set; } = null!;

        [ForeignKey(nameof(MechanicId))]
        public EmployeeProfile? Mechanic { get; set; } = null!;

        // Ссылка на справочник работ (WorkEntity)
        [Required]
        public Guid WorkId { get; set; }

        [ForeignKey(nameof(WorkId))]
        public WorkEntity Work { get; set; } = null!;

        // Ссылка на заказ-наряд (WorkOrderEntity)
        [Required]
        public Guid WorkOrderId { get; set; }

        [ForeignKey(nameof(WorkOrderId))]
        public WorkOrderEntity WorkOrder { get; set; } = null!;
    }
}
