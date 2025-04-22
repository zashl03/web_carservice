/*
 * Страница регистрации новых пользователей с интеграцией ASP.NET Core Identity.
 * Обеспечивает:
 * - Создание учетной записи с email и паролем
 * - Присвоение роли "Client" по умолчанию
 * - Создание профиля клиента
 * - Отправку подтверждения по email
 * - Поддержку внешних провайдеров аутентификации
 */
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using web_service.Data;
using web_service.Data.Entities;
using web_service.Data.Identity;

namespace web_service.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        // Зависимости Identity
        private readonly SignInManager<ApplicationUser> _signInManager; // Менеджер аутентификации
        private readonly UserManager<ApplicationUser> _userManager;     // Менеджер пользователей
        private readonly IUserStore<ApplicationUser> _userStore;        // Хранилище данных пользователей
        private readonly IUserEmailStore<ApplicationUser> _emailStore;  // Хранилище email
        private readonly ILogger<RegisterModel> _logger;                // Логгер системы
        private readonly IEmailSender _emailSender;                     // Сервис отправки email
        private readonly ApplicationDbContext _context;                 // Контекст БД приложения

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            // Инициализация зависимостей через DI
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();  // Получение специализированного хранилища для email
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }  // Модель данных формы регистрации

        public string ReturnUrl { get; set; }   // URL для перенаправления после регистрации
        public IList<AuthenticationScheme> ExternalLogins { get; set; } // Список внешних провайдеров

        // Модель данных для формы регистрации
        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }  // Основной идентификатор пользователя

            [Required]
            [Display(Name = "Полное имя")]
            public string FullName { get; set; } // Кастомное поле пользователя

            [Required]
            [Phone]
            [Display(Name = "Номер телефона")]
            public string PhoneNumber { get; set; } // Основной контактный номер

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } // Пароль с валидацией сложности

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } // Подтверждение пароля
        }

        // Обработка GET-запроса (отображение формы)
        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl; // Сохранение URL для перенаправления
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(); // Получение внешних провайдеров
        }

        // Обработка POST-запроса (отправка формы)
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/"); // URL по умолчанию - корень приложения
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser(); // Создание объекта пользователя через фабрику

                // Заполнение свойств пользователя
                user.FullName = Input.FullName;
                user.PhoneNumber = Input.PhoneNumber;
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None); // Установка логина
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);    // Установка email

                var result = await _userManager.CreateAsync(user, Input.Password); // Создание пользователя в БД
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Назначение роли "Client" по умолчанию
                    await _userManager.AddToRoleAsync(user, "Client");

                    // Создание профиля клиента
                    var profile = new ClientProfile { UserId = user.Id };
                    _context.ClientProfiles.Add(profile);
                    await _context.SaveChangesAsync();

                    // Генерация токена подтверждения email
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code)); // Кодирование токена

                    // Формирование URL подтверждения
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId, code, returnUrl },
                        protocol: Request.Scheme);

                    // Отправка письма подтверждения
                    await _emailSender.SendEmailAsync(
                        Input.Email,
                        "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    // Обработка сценариев подтверждения аккаунта
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false); // Автоматический вход
                        return LocalRedirect(returnUrl);
                    }
                }

                // Обработка ошибок создания пользователя
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page(); // Повторное отображение формы с ошибками
        }

        // Фабрика для создания объекта пользователя
        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>(); // Рефлексивное создание экземпляра
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'.");
            }
        }

        // Получение специализированного хранилища для работы с email
        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("Требуется хранилище с поддержкой email");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}