using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace web_service.Data.Entities
{
    public class PartInStorageEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid PartId { get; set; }

        [Required]
        public Guid StorageLocationId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [MaxLength(20)]
        public string MeasureUnit { get; set; }

        // Навигационные свойства
        public PartEntity Part { get; set; }
        public StorageLocationEntity StorageLocation { get; set; }
    }
}
