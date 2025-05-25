using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_service.Data.Entities
{
    public class PartEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ServicePn { get; set; }

        [Required]
        [MaxLength(50)]
        public string ManufacturerPn { get; set; }

        [Required]
        [MaxLength(100)]
        public string Manufacturer { get; set; }

        [Required]
        [MaxLength(100)]
        public string PartName { get; set; }

        public string Description { get; set; }

        [Required]
        [Column(TypeName = "numeric(10,2)")]
        public decimal Price { get; set; }

        [Required]
        [MaxLength(50)]
        public string OEMNumber { get; set; }

        [Required]
        [ForeignKey(nameof(Category))]
        public Guid CategoryId { get; set; } // Связь с категорией

        public CategoryPartEntity Category { get; set; } // Навигационное свойство
    }
}