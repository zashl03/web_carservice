/*
 * Файл содержит Entity-модель профиля сотрудника для БД.
 * Реализует связь 1-to-1 с пользователем (Identity) и хранит 
 * дополнительные данные для работников компании (отдел, табельный номер).
 * Используется для разделения ролей в системе и управления правами доступа.
 */
using web_service.Data.Identity;

namespace web_service.Data.Entities
{
    public class EmployeeProfile
    {
        public string UserId { get; set; }      // Внешний ключ для связи с ApplicationUser (формат: "guid|string_id")

        // Навигационное свойство для доступа к основным данным пользователя
        // Пример использования: employee.User.Email
        public ApplicationUser User { get; set; }

        // Табельный номер сотрудника (уникальный идентификатор в системе компании)
        public string TabNumber { get; set; }   // Формат: "Т-12345", обязателен для HR-систем

        // Отдел/подразделение сотрудника
        public string Department { get; set; }  // Примеры: "IT", "Бухгалтерия", "Служба поддержки"
    }
}