using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace web_service.Data.Entities
{
    public class CategoryPartEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; }

        [MaxLength(50)]
        public string ShortName { get; set; }

        [ForeignKey(nameof(ParentCategory))]
        public Guid? ParentId { get; set; } // Иерархия (null для корневых)

        public CategoryPartEntity ParentCategory { get; set; }
        public ICollection<CategoryPartEntity> Children { get; set; } = new List<CategoryPartEntity>();
    }
}
