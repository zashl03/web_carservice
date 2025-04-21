using web_service.Data.Identity;

namespace web_service.Data.Entities
{
    public class EmployeeProfile
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        // данные сотрудника
        public string TabNumber { get; set; }
        public string Department { get; set; }
    }
}
