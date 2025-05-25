using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace web_service.Data.Entities
{
    public class StorageLocationEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [MaxLength(50)]
        public string? NumberPlace { get; set; }

        [Required]
        [MaxLength(100)]
        public string Address { get; set; }

        [Required]
        [MaxLength(50)]
        public string Room { get; set; }

        [Required]
        [MaxLength(50)]
        public string Zone { get; set; }

        [Required]
        [MaxLength(50)]
        public string Rack { get; set; }

        [Required]
        public int Shelf { get; set; }

        [Required]
        public int Cell { get; set; }

        // Связь с кладовщиком
        [Required]
        [ForeignKey(nameof(Storekeeper))] // Связь с EmployeeProfile через UserId
        public string StorekeeperId { get; set; } // Тип string (совпадает с UserId)

        public EmployeeProfile Storekeeper { get; set; }
    }
}

