/*
 * Файл содержит модель для отображения данных автомобиля в API/UI.
 * Используется для передачи данных клиенту (только для чтения).
 * Содержит базовые поля автомобиля с локализованными названиями для отображения.
 * Валидация не требуется, так как данные уже прошли проверку при создании.
 */
using System.ComponentModel.DataAnnotations;

namespace web_service.Models.Car
{
    public class CarViewModel
    {
        public Guid Id { get; set; }         // Уникальный идентификатор автомобиля (из БД)

        [Display(Name = "Марка")]
        public string Brand { get; set; }    // Марка автомобиля (пример: "BMW")

        [Display(Name = "Модель")]
        public string Model { get; set; }    // Модель автомобиля (пример: "X5")

        [Display(Name = "VIN")]
        public string VIN { get; set; }      // VIN-код (уникальный, 17 символов)
    }
}