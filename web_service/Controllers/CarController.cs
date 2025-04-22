/*
 * Контроллер для управления автомобилями клиентов.
 * Обеспечивает:
 * - Просмотр списка автомобилей клиента
 * - Добавление новых автомобилей
 * - Интеграцию с системой аутентификации и профилями
 * Доступен только пользователям с ролью "Client"
 */
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_service.Data;
using web_service.Data.Identity;
using web_service.Domain.Entities;
using web_service.Models.Car;

[Authorize(Roles = "Client")]  // Ограничение доступа только для клиентов
public class CarController : Controller
{
    // Сервис для бизнес-логики работы с автомобилями
    private readonly ICarService _carService;

    // Менеджер пользователей для работы с Identity
    private readonly UserManager<ApplicationUser> _userManager;

    // Контекст БД для прямых запросов к данным
    private readonly ApplicationDbContext _context;

    // Автомаппер для преобразования моделей
    private readonly IMapper _mapper;

    // Конструктор с внедрением зависимостей через DI
    public CarController(
        ICarService carService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IMapper mapper)
    {
        _carService = carService;
        _userManager = userManager;
        _context = context;
        _mapper = mapper;
    }

    // GET: /Car/Index - Главная страница со списком автомобилей
    public async Task<IActionResult> Index()
    {
        // Получение текущего авторизованного пользователя
        var user = await _userManager.GetUserAsync(User);

        // Поиск привязанного профиля клиента
        var clientProfile = await _context.ClientProfiles
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        if (clientProfile == null)
        {
            return NotFound("Профиль клиента не найден");  // Защита от несвязанных профилей
        }

        // Получение автомобилей через сервисный слой
        var cars = await _carService.GetCarsByOwnerAsync(clientProfile.UserId);

        // Преобразование в ViewModel для отображения
        var vm = _mapper.Map<IEnumerable<CarViewModel>>(cars);
        return View(vm);
    }

    // GET: /Car/Add - Форма добавления нового автомобиля
    [HttpGet]
    public IActionResult Add() => View();  // Возвращает пустую форму

    // POST: /Car/Add - Обработка данных формы
    [HttpPost]
    public async Task<IActionResult> Add(AddCarViewModel model)
    {
        if (!ModelState.IsValid) return View(model);  // Валидация модели на сервере

        // Повторная проверка профиля клиента (защита от изменения состояния)
        var user = await _userManager.GetUserAsync(User);
        var clientProfile = await _context.ClientProfiles
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        if (clientProfile == null)
        {
            ModelState.AddModelError("", "Профиль клиента не найден");
            return View(model);
        }

        try
        {
            // Преобразование ViewModel -> Domain Model
            var car = _mapper.Map<Car>(model);

            // Добавление автомобиля через сервис
            await _carService.AddCarAsync(car, clientProfile.UserId);

            // Уведомление об успехе через TempData
            TempData["SuccessMessage"] = "Автомобиль успешно добавлен!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)  // Обработка ошибок бизнес-логики
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }
}