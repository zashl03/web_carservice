using System.Collections.Generic;
using web_service.Data.Identity;

namespace web_service.Data.Entities
{
    public class ClientProfile
    {
        public string UserId { get; set; }  // Внешний ключ для ApplicationUser
        public ApplicationUser User { get; set; }

        // Коллекция автомобилей клиента
        public ICollection<CarEntity> Cars { get; set; } = new List<CarEntity>();
    }
}