/*
 * Файл содержит Entity-модель профиля клиента для БД.
 * Реализует связи:
 * - 1-to-1 с пользователем (Identity)
 * - 1-to-many с автомобилями клиента
 * Хранит дополнительные данные, специфичные для клиентской роли пользователя.
 */
using System.Collections.Generic;
using web_service.Data.Identity;

namespace web_service.Data.Entities
{
    public class ClientProfile
    {
        public string UserId { get; set; }  // Внешний ключ для связи с ApplicationUser (формат: GUID или строковый ID)

        // Навигационное свойство для доступа к данным пользователя
        // Загружается через .Include() в EF Core (например: _db.ClientProfiles.Include(c => c.User))
        public ApplicationUser User { get; set; }

        // Коллекция автомобилей клиента (отношение 1 ко многим)
        // Инициализация предотвращает NullReferenceException при добавлении авто новому клиенту
        public ICollection<CarEntity> Cars { get; set; } = new List<CarEntity>(); // Пример: [CarEntity{Id=...}, ...]
    }
}