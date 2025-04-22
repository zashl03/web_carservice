/*
 * Razor Page для управления автомобилями в личном кабинете пользователя.
 * Обеспечивает:
 * - Просмотр списка привязанных автомобилей
 * - Добавление новых автомобилей через форму
 * - Интеграцию с профилем клиента (автосоздание при отсутствии)
 * Доступна только авторизованным пользователям
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using web_service.Data;
using web_service.Data.Identity;
using web_service.Domain.Entities;
using web_service.Models.Car;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using web_service.Data.Entities;

namespace web_service.Areas.Identity.Pages.Account.Manage
{
    [Authorize]  // Требуется авторизация любого типа
    public class MyCarsModel : PageModel
    {
        private readonly ICarService _carService;      // Сервис для работы с автомобилями
        private readonly ApplicationDbContext _context; // Контекст БД для прямых запросов
        private readonly UserManager<ApplicationUser> _userManager; // Менеджер пользователей
        private readonly IMapper _mapper;              // Преобразователь моделей

        [BindProperty]                                 // Привязка модели из формы POST
        public AddCarViewModel Input { get; set; }     // Модель данных для добавления авто

        // Список автомобилей для отображения в таблице
        public IEnumerable<CarViewModel> Cars { get; set; } = new List<CarViewModel>();

        public MyCarsModel(
            ICarService carService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            // Инициализация зависимостей, переданных через DI
            _carService = carService;
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        // Обработка GET-запроса (загрузка страницы)
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login"); // Редирект, если пользователь не авторизован
            }

            // Поиск профиля клиента с привязанными автомобилями
            var clientProfile = await _context.ClientProfiles
                .Include(c => c.Cars)  // Жадная загрузка автомобилей
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            // Автосоздание профиля при первом входе
            if (clientProfile == null)
            {
                clientProfile = new ClientProfile { UserId = user.Id };
                _context.ClientProfiles.Add(clientProfile);
                await _context.SaveChangesAsync();  // Сохраняем новый профиль
            }

            // Маппинг Entity -> ViewModel для отображения
            Cars = _mapper.Map<IEnumerable<CarViewModel>>(clientProfile.Cars);
            return Page();
        }

        // Обработка POST-запроса (добавление автомобиля)
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();  // Возврат формы с ошибками валидации
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                var clientProfile = await _context.ClientProfiles
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);

                var car = _mapper.Map<Car>(Input);  // Преобразование DTO -> Domain Model
                await _carService.AddCarAsync(car, clientProfile.UserId);

                TempData["StatusMessage"] = "Автомобиль добавлен!"; // Сообщение об успехе
                return RedirectToPage();  // Паттерн POST-REDIRECT-GET
            }
            catch (Exception ex)  // Обработка ошибок из сервиса
            {
                ModelState.AddModelError("", ex.Message);
                await OnGetAsync();  // Перезагрузка списка автомобилей
                return Page();
            }
        }
    }
}