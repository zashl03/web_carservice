/*
 * Файл содержит кастомную модель пользователя для системы аутентификации ASP.NET Core Identity.
 * Расширяет базовый класс IdentityUser, добавляя дополнительные поля и связи:
 * - Полное имя пользователя
 * - Профили клиента/сотрудника (разделение ролей)
 * - Связи с дочерними сущностями через навигационные свойства
 */
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using web_service.Data.Entities;

namespace web_service.Data.Identity
{
    public class ApplicationUser : IdentityUser  // Наследование от стандартного IdentityUser
    {
        // Полное имя пользователя (не разбитое на First/Last name)
        public string FullName { get; set; }      // Пример: "Иванов Иван Иванович"

        // Переопределение номера телефона (базовое свойство уже есть в IdentityUser)
        public new string PhoneNumber { get; set; } // Явное объявление для кастомизации

        // Навигационное свойство для связи с профилем клиента (отношение 1-to-1)
        public ClientProfile ClientProfile { get; set; } // Заполняется, если пользователь - клиент

        // Навигационное свойство для связи с профилем сотрудника (отношение 1-to-1)
        public EmployeeProfile EmployeeProfile { get; set; } // Заполняется, если пользователь - сотрудник
    }
}