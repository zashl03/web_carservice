using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using web_service.Data;
using web_service.Data.Entities;
using web_service.Data.Identity;
using web_service.Models.Admin;

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

        // Список сотрудников для таблицы
        public List<EmployeeListViewModel> Employees { get; set; } = new();

        // Модель, общая для создания и редактирования
        [BindProperty]
        public InputModel Input { get; set; } = new();

        // Список ролей для <select>
        public SelectList RoleSelectList { get; set; } = null!;

        // GET: подгрузка сотрудников + ролей
        public async Task OnGetAsync()
        {
            Employees.Clear();

            var users = await _userMgr.Users.ToListAsync();
            foreach (var u in users)
            {
                var roles = await _userMgr.GetRolesAsync(u);
                var profile = await _db.EmployeeProfiles
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync(p => p.UserId == u.Id);

                Employees.Add(new EmployeeListViewModel
                {
                    UserId = u.Id,
                    Email = u.Email!,
                    Roles = roles.ToList(),
                    TabNumber = profile?.TabNumber,
                    Position = profile?.Position
                });
            }

            var allRoles = new[] { "Administrator", "Storekeeper", "Mechanic" };
            RoleSelectList = new SelectList(allRoles);
        }

        // POST: создание нового сотрудника
        public async Task<IActionResult> OnPostCreateAsync()
        {
            // валидация: при создании Password обязателен, UserId должен быть пустым
            ModelState.Remove(nameof(Input.UserId));
            if (string.IsNullOrEmpty(Input.Password))
            {
                ModelState.AddModelError(nameof(Input.Password), "Пароль обязателен.");
            }

            await OnGetAsync(); // чтобы в случае ошибки таблица была заполнена

            if (!ModelState.IsValid)
                return Page();

            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                FullName = Input.Email,
                PhoneNumber = ""
            };

            var createResult = await _userMgr.CreateAsync(user, Input.Password!);
            if (!createResult.Succeeded)
            {
                foreach (var e in createResult.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return Page();
            }

            await _userMgr.AddToRoleAsync(user, Input.Role);

            // Подтверждаем e-mail
            var token = await _userMgr.GenerateEmailConfirmationTokenAsync(user);
            await _userMgr.ConfirmEmailAsync(user, token);

            // Сохраняем профиль сотрудника
            var employeeProfile = new EmployeeProfile
            {
                UserId = user.Id,
                TabNumber = Input.TabNumber,
                Position = Input.Position
            };

            _db.EmployeeProfiles.Add(employeeProfile);
            await _db.SaveChangesAsync();

            return RedirectToPage();
        }

        // AJAX GET: детали для модального редактирования
        public async Task<JsonResult> OnGetEmployeeDetailsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new JsonResult(null);

            var user = await _userMgr.Users
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return new JsonResult(null);

            var roles = await _userMgr.GetRolesAsync(user);
            var profile = await _db.EmployeeProfiles
                                   .AsNoTracking()
                                   .FirstOrDefaultAsync(p => p.UserId == userId);

            var result = new
            {
                userId = user.Id,
                email = user.Email,
                role = roles.FirstOrDefault() ?? "",
                tabNumber = profile?.TabNumber,
                position = profile?.Position
            };

            return new JsonResult(result);
        }

        // POST: редактирование существующего сотрудника
        public async Task<IActionResult> OnPostEditAsync()
        {
            // валидация: при редактировании Password игнорируем, UserId обязателен
            ModelState.Remove(nameof(Input.Password));
            if (string.IsNullOrEmpty(Input.UserId))
            {
                ModelState.AddModelError(nameof(Input.UserId), "UserId обязателен.");
            }

            await OnGetAsync(); // чтобы таблица была доступна при ошибке

            if (!ModelState.IsValid)
                return Page();

            var user = await _userMgr.FindByIdAsync(Input.UserId!);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Пользователь не найден.");
                return Page();
            }

            var profile = await _db.EmployeeProfiles
                                   .FirstOrDefaultAsync(p => p.UserId == Input.UserId);
            if (profile == null)
            {
                ModelState.AddModelError(string.Empty, "Профиль сотрудника не найден.");
                return Page();
            }

            // Обновляем e-mail / UserName
            user.Email = Input.Email;
            user.UserName = Input.Email;
            var updateResult = await _userMgr.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var e in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return Page();
            }

            // Обновляем роль, если изменилась
            var currentRoles = await _userMgr.GetRolesAsync(user);
            if (!currentRoles.Contains(Input.Role))
            {
                await _userMgr.RemoveFromRolesAsync(user, currentRoles);
                await _userMgr.AddToRoleAsync(user, Input.Role);
            }

            // Обновляем профиль
            profile.TabNumber = Input.TabNumber;
            profile.Position = Input.Position;
            _db.EmployeeProfiles.Update(profile);
            await _db.SaveChangesAsync();

            return RedirectToPage();
        }

        // POST: удалить сотрудника
        public async Task<IActionResult> OnPostDeleteAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage();

            var user = await _userMgr.FindByIdAsync(userId);
            if (user != null)
            {
                var profile = await _db.EmployeeProfiles
                                       .FirstOrDefaultAsync(p => p.UserId == userId);
                if (profile != null)
                {
                    _db.EmployeeProfiles.Remove(profile);
                    await _db.SaveChangesAsync();
                }

                var roles = await _userMgr.GetRolesAsync(user);
                if (roles.Any())
                {
                    await _userMgr.RemoveFromRolesAsync(user, roles);
                }

                await _userMgr.DeleteAsync(user);
            }

            return RedirectToPage();
        }

        // Единая модель для создания/редактирования
        public class InputModel
        {
            // При создании остаётся null, при редактировании заполняется из JS
            public string? UserId { get; set; }

            [Required, EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Пароль")]
            public string? Password { get; set; } // нужен только при создании

            [Required]
            [Display(Name = "Роль")]
            public string Role { get; set; } = string.Empty;

            [Required(ErrorMessage = "Табельный номер обязателен")]
            [Display(Name = "Табельный номер")]
            public string TabNumber { get; set; } = string.Empty;

            [Display(Name = "Должность")]
            public string? Position { get; set; }
        }
    }
}
