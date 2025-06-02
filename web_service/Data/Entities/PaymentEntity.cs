// web_service/Data/Entities/PaymentEntity.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_service.Data.Entities
{
    public class PaymentEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(20)]
        [Display(Name = "Способ оплаты")]
        public string PaymentType { get; set; } = null!; // Например: "Банковская карта", "Наличные", "QR код"

        [Required]
        [Display(Name = "Дата оплаты")]
        public DateTime DatePayment { get; set; }

        [Required]
        [Column(TypeName = "numeric(10,2)")]
        [Display(Name = "Итоговая сумма")]
        public decimal FinalCost { get; set; }

        // Внешний ключ на WorkOrder
        [Required]
        public Guid WorkOrderId { get; set; }

        [ForeignKey(nameof(WorkOrderId))]
        public WorkOrderEntity WorkOrder { get; set; } = null!;

        // Внешний ключ на профиль администратора (сотрудника), который подтвердил оплату
        [Required]
        public string AdministratorId { get; set; } = null!;

        [ForeignKey(nameof(AdministratorId))]
        public EmployeeProfile Administrator { get; set; } = null!;
    }
}
