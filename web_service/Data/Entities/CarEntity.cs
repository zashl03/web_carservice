using web_service.Data.Identity;

namespace web_service.Data.Entities
{
    public class CarEntity
    {
        public Guid Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string VIN { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
