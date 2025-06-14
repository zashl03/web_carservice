// Index.cshtml.cs
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using web_service.Data.Identity;

namespace web_service.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "ФИО")]
            [Required(ErrorMessage = "Поле ФИО обязательно.")]
            public string FullName { get; set; }

            [Phone]
            [Display(Name = "Номер телефона")]
            public string PhoneNumber { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            Username = await _userManager.GetUserNameAsync(user);
            // Читаем напрямую из user.PhoneNumber
            var phone = user.PhoneNumber;
            var fullName = user.FullName;

            Input = new InputModel
            {
                FullName = fullName ?? string.Empty,
                PhoneNumber = phone ?? string.Empty
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Не удалось загрузить пользователя с ID '{_userManager.GetUserId(User)}'.");

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Не удалось загрузить пользователя с ID '{_userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var needsUpdate = false;

            // Обновляем FullName, если изменилось
            if (Input.FullName != user.FullName)
            {
                user.FullName = Input.FullName;
                needsUpdate = true;
            }

            // Обновляем напрямую user.PhoneNumber, если изменился
            if (Input.PhoneNumber != user.PhoneNumber)
            {
                user.PhoneNumber = Input.PhoneNumber;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    StatusMessage = "Ошибка при обновлении данных профиля.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Профиль успешно обновлен";
            return RedirectToPage();
        }
    }
}
