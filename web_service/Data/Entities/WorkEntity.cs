using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_service.Data.Entities
{
    public class WorkEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        [Display(Name = "Название работы")]
        public string WorkName { get; set; } = null!;

        [MaxLength(500)]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "numeric(10,2)")]
        [Display(Name = "Цена")]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Длительность (в минутах)")]
        public int Duration { get; set; }
    }
}
