using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace web_service.Models.Admin
{
    public class CreateEmployeeViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }

        public IEnumerable<string> AvailableRoles { get; set; }
    }
}
