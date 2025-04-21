using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_service.Data;
using web_service.Data.Identity;
using web_service.Domain.Entities;
using web_service.Models.Car;

[Authorize(Roles = "Client")]
public class CarController : Controller
{
    private readonly ICarService _carService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

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

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var clientProfile = await _context.ClientProfiles
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        if (clientProfile == null)
        {
            return NotFound("Профиль клиента не найден");
        }

        var cars = await _carService.GetCarsByOwnerAsync(clientProfile.UserId);
        var vm = _mapper.Map<IEnumerable<CarViewModel>>(cars);
        return View(vm);
    }

    [HttpGet]
    public IActionResult Add() => View();

    [HttpPost]
    public async Task<IActionResult> Add(AddCarViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

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
            var car = _mapper.Map<Car>(model);
            await _carService.AddCarAsync(car, clientProfile.UserId);
            TempData["SuccessMessage"] = "Автомобиль успешно добавлен!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }
}