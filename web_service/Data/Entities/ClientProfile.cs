using web_service.Data.Identity;

namespace web_service.Data.Entities
{
    public class ClientProfile
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        // здесь только дополнительные данные клиента
        public string PreferredService { get; set; }
    }

}
