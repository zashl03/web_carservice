using System;
using System.ComponentModel.DataAnnotations;
using web_service.Data.Identity;

namespace web_service.Data.Entities
{
    public class WarehouseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(200)]
        public string Address { get; set; }

        // Внешний ключ на кладовщика (EmployeeProfile.UserId)
        [Required]
        public string StorekeeperId { get; set; }

        // Навигационное свойство на профиль сотрудника
        public EmployeeProfile Storekeeper { get; set; }
    }
}