
namespace web_service.Models.Admin
{
    public class EmployeeListViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();

        // Новые поля из EmployeeProfile
        public string? TabNumber { get; set; }
        public string? Position { get; set; }
    }
}
