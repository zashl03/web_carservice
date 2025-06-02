using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_service.Data.Entities
{
    public class WorkOrderEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(20)]
        [Display(Name = "Номер заказ-наряда")]
        public string WorkOrderNumber { get; set; } = null!;

        [Column(TypeName = "numeric(10,2)")]
        [Display(Name = "Стоимость")]
        public decimal? Cost { get; set; }

        [Required]
        [Display(Name = "Дата открытия")]
        public DateTime DateOpened { get; set; }

        [Required]
        [MaxLength(20)]
        [Display(Name = "Статус")]
        public string Status { get; set; } = null!;

        // Ссылка на RecordEntity
        [Required]
        public Guid RecordId { get; set; }

        [ForeignKey(nameof(RecordId))]
        public RecordEntity Record { get; set; } = null!;
    }
}
