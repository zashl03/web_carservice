using System.ComponentModel.DataAnnotations;

namespace web_service.Models.Car
{
    public class CarViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Марка")]
        public string Brand { get; set; }

        [Display(Name = "Модель")]
        public string Model { get; set; }

        [Display(Name = "VIN")]
        public string VIN { get; set; }
    }
}
