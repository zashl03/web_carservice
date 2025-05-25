using web_service.Data.Entities;
using web_service.Data.Identity;

namespace web_service.Domain.Entities
{
    public class ServiceRecord
    {
        public Guid Id { get; set; }
        public DateTime BookingDate { get; set; }
        public string? Comment { get; set; }

        public Guid CarId { get; set; }
        public Car Car { get; set; }

        public string UserId { get; set; }
        public ClientProfile Client { get; set; }
    }
}
