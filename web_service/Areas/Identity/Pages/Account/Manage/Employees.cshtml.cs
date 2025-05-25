using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web_service.Data;
using web_service.Data.Identity;
using web_service.Models.Admin;
using System.ComponentModel.DataAnnotations;
using web_service.Data.Entities;

namespace web_service.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles = "Administrator")]
    public class EmployeesModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userMgr;

        public EmployeesModel(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userMgr)
        {
            _db = db;
            _userMgr = userMgr;
        }

        // Список существующих сотрудников
        public List<EmployeeListViewModel> Employees { get; set; } = new();

        // Данные формы
        [BindProperty]
        public InputModel Input { get; set; } = new();

        // Список ролей для dropdown
        public SelectList RoleSelectList { get; set; }

        // Получение данных
        public async Task OnGetAsync()
        {
            // Заполняем таблицу
            var users = await _userMgr.Users.ToListAsync();
            foreach (var u in users)
            {
                var roles = await _userMgr.GetRolesAsync(u);
                var profile = await _db.EmployeeProfiles.FirstOrDefaultAsync(p => p.UserId == u.Id);
                Employees.Add(new EmployeeListViewModel
                {
                    Email = u.Email!,
                    Roles = roles.ToList(),
                    TabNumber = profile?.TabNumber,
                    Department = profile?.Department
                });
            }

            // Подготавливаем список ролей
            var allRoles = new[] { "Administrator", "Storekeeper", "Mechanic" };
            RoleSelectList = new SelectList(allRoles);
        }

        // Обработка формы
        public async Task<IActionResult> OnPostAsync()
        {
            await OnGetAsync();

            if (!ModelState.IsValid)
                return Page();

            // Создаём пользователя (без изменений)
            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                FullName = Input.Email,
                PhoneNumber = ""
            };

            var createResult = await _userMgr.CreateAsync(user, Input.Password);
            if (!createResult.Succeeded)
            {
                foreach (var e in createResult.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return Page();
            }

            // Назначаем роль (без изменений)
            await _userMgr.AddToRoleAsync(user, Input.Role);

            // Подтверждаем e-mail (без изменений)
            var token = await _userMgr.GenerateEmailConfirmationTokenAsync(user);
            await _userMgr.ConfirmEmailAsync(user, token);

            // ДОБАВЛЕНО: Сохраняем профиль сотрудника
            var employeeProfile = new EmployeeProfile
            {
                UserId = user.Id,           // Связь с созданным пользователем
                TabNumber = Input.TabNumber,
                Department = Input.Department
            };

            _db.EmployeeProfiles.Add(employeeProfile);
            await _db.SaveChangesAsync();  // Сохраняем в БД

            return RedirectToPage();
        }

        // Модель для формы
        public class InputModel
        {
            [Required, EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required, DataType(DataType.Password)]
            [Display(Name = "Пароль")]
            public string Password { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Роль")]
            public string Role { get; set; } = string.Empty;

            // ДОБАВЛЕНО: Поля для EmployeeProfile
            [Required(ErrorMessage = "Табельный номер обязателен")]
            [Display(Name = "Табельный номер")]
            public string TabNumber { get; set; } = string.Empty;

            [Display(Name = "Отдел")]
            public string? Department { get; set; }
        }
    }
}
