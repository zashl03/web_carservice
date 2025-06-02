/*
 * Файл содержит Entity-модель профиля сотрудника для БД.
 * Реализует связь 1-to-1 с пользователем (Identity) и хранит
 * дополнительные данные для работников компании.
 */
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using web_service.Data.Identity;

namespace web_service.Data.Entities
{
    public class EmployeeProfile
    {
        [Key]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = null!;  // Первичный и внешний ключ на ApplicationUser

        // Навигационное свойство к ApplicationUser
        public ApplicationUser User { get; set; } = null!;

        [Required]
        public string TabNumber { get; set; } = null!; // Табельный номер сотрудника

        public string? Position { get; set; } = null!; // Отдел/подразделение сотрудника
    }
}
