/*
 * Файл содержит Entity-модель профиля клиента для БД.
 * Реализует связи:
 * - 1-to-1 с пользователем (Identity)
 * - 1-to-many с автомобилями клиента
 * Хранит дополнительные данные, специфичные для клиентской роли пользователя.
 */
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using web_service.Data.Identity;

namespace web_service.Data.Entities
{
    public class ClientProfile
    {
        [Key]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = null!;

        [Required]
        public DateTime DateCreated { get; set; }

        // Навигационное свойство к ApplicationUser
        public ApplicationUser User { get; set; } = null!;

        // Коллекция автомобилей клиента (отношение 1 ко многим)
        public ICollection<CarEntity> Cars { get; set; } = new List<CarEntity>();

        public ClientProfile()
        {
            // EF Core вызовет этот конструктор при создании новой сущности
            DateCreated = DateTime.UtcNow;
        }
    }
}
