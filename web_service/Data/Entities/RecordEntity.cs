/*
 * Файл содержит Entity-модель автомобиля для работы с базой данных.
 * Определяет структуру таблицы Cars: поля, ограничения и связи между сущностями.
 * Используется Entity Framework Core для маппинга объектов C# на реляционную БД.
 * Включает:
 * - Валидацию данных через атрибуты (Required/MaxLength)
 * - Связь many-to-one с сущностью ClientProfile (один владелец → много авто)
 * - Автогенерацию первичного ключа (Id)
 */
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_service.Data.Entities
{
    public class RecordEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CarId { get; set; }

        [ForeignKey(nameof(CarId))]
        public virtual CarEntity Car { get; set; } = null!;

        [Required]
        public DateTime BookingDate { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }
    }
}
