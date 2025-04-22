using System.ComponentModel.DataAnnotations;
/*
 * [Required] - обязательное поле
 * [StringLength] - ограничение длины
 * [Display] - название поля для UI/сообщений
 * [RegularExpression] - проверка формата
 */
namespace web_service.Models.Car
{
    public class AddCarViewModel
    {
        [Required(ErrorMessage = "Поле 'Марка' обязательно для заполнения")]
        [StringLength(50, ErrorMessage = "Марка не может быть длиннее 50 символов")]
        [Display(Name = "Марка")]
        public string Brand { get; set; } // Марка автомобиля (обязательное поле, макс. 50 символов)

        [Required(ErrorMessage = "Поле 'Модель' обязательно для заполнения")]
        [StringLength(50, ErrorMessage = "Модель не может быть длиннее 50 символов")]
        [Display(Name = "Модель")]
        public string Model { get; set; } // Модель автомобиля (обязательное поле, макс. 50 символов)

        [Required(ErrorMessage = "Поле 'VIN' обязательно для заполнения")]
        [RegularExpression(@"^[A-HJ-NPR-Z0-9]{17}$",
            ErrorMessage = "VIN должен состоять из 17 символов (латинские буквы и цифры)")]
        [Display(Name = "VIN")]
        public string VIN { get; set; } // VIN-код (17 символов, формат: буквы A-Z кроме I,O,Q + цифры)
    }
}