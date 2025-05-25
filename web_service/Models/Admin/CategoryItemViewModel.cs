using web_service.Data.Entities;

namespace web_service.Models.Admin
{
    public class CategoryItemViewModel
    {
        public CategoryPartEntity Category { get; set; }
        public int Level { get; set; }
    }
}
