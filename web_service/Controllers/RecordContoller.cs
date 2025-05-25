// RecordController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using web_service.Data.Identity;
using web_service.Models.Record;
using web_service.Services;

[Authorize(Roles = "Client")]
public class RecordController : Controller
{
    private readonly IRecordService _recordService;
    private readonly ICarService _carService;
    private readonly UserManager<ApplicationUser> _userManager;

    public RecordController(
        IRecordService recordService,
        ICarService carService,
        UserManager<ApplicationUser> userManager)
    {
        _recordService = recordService;
        _carService = carService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Add()
    {
        var userId = _userManager.GetUserId(User);
        var cars = await _carService.GetCarsByOwnerAsync(userId);
        var vm = new AddRecordViewModel
        {
            Cars = cars.Select(c => new SelectListItem
            {
                Text = $"{c.Brand} {c.Model} ({c.VIN})",
                Value = c.Id.ToString()
            }).ToList(),
            BookingDate = DateTime.Now.AddDays(1) // например, по умолчанию завтра
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Add(AddRecordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // пересоздать список машин, если валидация не прошла
            var userId = _userManager.GetUserId(User);
            var cars = await _carService.GetCarsByOwnerAsync(userId);
            model.Cars = cars.Select(c => new SelectListItem
            {
                Text = $"{c.Brand} {c.Model} ({c.VIN})",
                Value = c.Id.ToString()
            }).ToList();
            return View(model);
        }

        var userId2 = _userManager.GetUserId(User);
        await _recordService.CreateRecordAsync(
            userId2,
            model.SelectedCarId,
            model.BookingDate,
            model.Comment ?? string.Empty
        );
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var records = await _recordService.GetUserRecordsAsync(userId);
        return View(records);
    }
}
