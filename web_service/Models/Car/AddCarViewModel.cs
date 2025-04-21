using System.ComponentModel.DataAnnotations;

namespace web_service.Models.Car
{
    public class AddCarViewModel
    {
        [Required(ErrorMessage = "Поле 'Марка' обязательно для заполнения")]
        [StringLength(50, ErrorMessage = "Марка не может быть длиннее 50 символов")]
        [Display(Name = "Марка")]
        public string Brand { get; set; }

        [Required(ErrorMessage = "Поле 'Модель' обязательно для заполнения")]
        [StringLength(50, ErrorMessage = "Модель не может быть длиннее 50 символов")]
        [Display(Name = "Модель")]
        public string Model { get; set; }

        [Required(ErrorMessage = "Поле 'VIN' обязательно для заполнения")]
        [RegularExpression(@"^[A-HJ-NPR-Z0-9]{17}$",
            ErrorMessage = "VIN должен состоять из 17 символов (латинские буквы и цифры)")]
        [Display(Name = "VIN")]
        public string VIN { get; set; }
    }
}