using System.ComponentModel.DataAnnotations;

namespace web_service.Data.Entities
{
    public class CarEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string Brand { get; set; }

        [Required, MaxLength(50)]
        public string Model { get; set; }

        [Required, MaxLength(17)]
        public string VIN { get; set; }

        // Внешний ключ для связи с клиентом
        public string ClientProfileId { get; set; }
        public ClientProfile Client { get; set; }
    }
}