using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_service.Data.Entities
{
    public class PartInWorkEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        // Ссылка на WorkTask
        [Required]
        public Guid WorkTaskId { get; set; }
        [ForeignKey(nameof(WorkTaskId))]
        public WorkTaskEntity WorkTask { get; set; } = null!;

        // Ссылка на Part (справочник запчастей)
        [Required]
        public Guid PartId { get; set; }
        [ForeignKey(nameof(PartId))]
        public PartEntity Part { get; set; } = null!;

        // Ссылка на кладовщика (EmployeeProfile)
        [Required]
        public string? StorekeeperId { get; set; } = null!;
        [ForeignKey(nameof(StorekeeperId))]
        public EmployeeProfile? Storekeeper { get; set; } = null!;

        // Количество использованных единиц
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть >= 1")]
        public int Quantity { get; set; }

        // Фактическая стоимость этих запчастей
        [Required]
        [Column(TypeName = "numeric(10,2)")]
        public decimal Cost { get; set; }
    }
}
