/*
 * Файл содержит Entity-модель автомобиля для работы с базой данных.
 * Определяет структуру таблицы Cars: поля, ограничения и связи между сущностями.
 * Используется Entity Framework Core для маппинга объектов C# на реляционную БД.
 * Включает:
 * - Валидацию данных через атрибуты (Required/MaxLength)
 * - Связь many-to-one с сущностью ClientProfile (один владелец → много авто)
 * - Автогенерацию первичного ключа (Id)
 */
using System.ComponentModel.DataAnnotations;

namespace web_service.Data.Entities
{
    public class CarEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Уникальный идентификатор (первичный ключ), генерируется автоматически

        [Required, MaxLength(50)]                       // Обязательное поле, макс. длина 50 символов (ограничение БД)
        public string Brand { get; set; }               // Марка автомобиля (хранится в БД)

        [Required, MaxLength(50)]                       // Аналогичные ограничения для модели
        public string Model { get; set; }                // Модель автомобиля (пример: "Corolla")

        [Required, MaxLength(17)]                        // Строго 17 символов (стандарт VIN)
        public string VIN { get; set; }                  // Уникальный идентификатор ТС (без дубликатов в БД)

        // Внешний ключ для связи с таблицей ClientProfiles (формат: "user_12345")
        public string ClientProfileId { get; set; }      // ID владельца в виде строки (связь many-to-one)

        // Навигационное свойство для загрузки связанного владельца через EF Core
        public ClientProfile Client { get; set; }        // Объект владельца (ленивая/явная загрузка)
    }
}