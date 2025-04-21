using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using web_service.Data.Entities;

namespace web_service.Data.Identity
{
    public class ApplicationUser: IdentityUser
    {
        public string FullName { get; set; }
        public string PhoneNumber {  get; set; }

        public ClientProfile ClientProfile { get; set; }
        public EmployeeProfile EmployeeProfile { get; set; }
    }
}
