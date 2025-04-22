/*
 * Контроллер для обработки основных страниц приложения:
 * - Главная страница
 * - Страница "Конфиденциальность"
 * - Обработка ошибок
 * Наследуется от базового Controller, использует встроенный логгер для диагностики.
 */
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using web_service.Models;

namespace web_service.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;  // Встроенный логгер для записи событий

        // Конструктор с внедрением зависимости логгера через DI
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _logger.LogInformation("HomeController initialized");  // Пример логирования
        }

        // Обрабатывает GET-запросы на главную страницу (/Home/Index)
        // Возвращает стандартное представление Views/Home/Index.cshtml
        public IActionResult Index()
        {
            _logger.LogDebug("Loading home page");  // Диагностическое сообщение
            return View();
        }

        // Обрабатывает GET-запросы на страницу конфиденциальности (/Home/Privacy)
        // Возвращает представление Views/Home/Privacy.cshtml
        public IActionResult Privacy()
        {
            _logger.LogDebug("Loading privacy page");
            return View();
        }

        // Обрабатывает все необработанные исключения в приложении
        // Атрибут отключает кэширование ответов об ошибках
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Создаем модель ошибки с ID запроса для диагностики
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier // Уникальный идентификатор запроса
            };

            _logger.LogError($"Error occurred. Request ID: {errorViewModel.RequestId}");  // Логирование ошибки
            return View(errorViewModel);  // Возвращаем представление Views/Shared/Error.cshtml
        }
    }
}