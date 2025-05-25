using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using web_service.Data;
using web_service.Data.Identity;
using web_service.Data.Entities;
using web_service.Models.Record;
using web_service.Services;
using System.ComponentModel.DataAnnotations;

namespace web_service.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles = "Client")]
    public class RecordModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICarService _carService;
        private readonly ILogger<RecordModel> _logger;

        public RecordModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ICarService carService,
            ILogger<RecordModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _carService = carService;
            _logger = logger;
        }

        [BindProperty]
        public RecordInputModel Input { get; set; } = new();

        public List<RecordViewModel> Records { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            try
            {
                var user = await GetCurrentUserAsync();
                // проверка принадлежности автомобиля через CarService
                var cars = await _carService.GetCarsByOwnerAsync(user.Id);
                if (!cars.Any(c => c.Id == Input.SelectedCarId))
                    throw new ValidationException("Выбранный автомобиль не найден");

                var recordEntity = new RecordEntity
                {
                    Id = Guid.NewGuid(),
                    CarId = Input.SelectedCarId,
                    BookingDate = DateTime.SpecifyKind(Input.BookingDate, DateTimeKind.Utc),
                    Comment = Input.Comment
                };

                _context.Records.Add(recordEntity);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Запись успешно создана!";
                return RedirectToPage();
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadDataAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании записи");
                ModelState.AddModelError(string.Empty, "Произошла ошибка при сохранении");
                await LoadDataAsync();
                return Page();
            }
        }

        private async Task LoadDataAsync()
        {
            var user = await GetCurrentUserAsync();

            // Загрузка списка автомобилей через CarService
            var cars = await _carService.GetCarsByOwnerAsync(user.Id);
            Input.Cars = cars.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Brand} {c.Model} ({c.VIN})"
            }).ToList();

            // Загрузка записей на обслуживание клиента
            Records = await _context.Records
                .Include(r => r.Car)
                .Where(r => r.Car.ClientProfileId == user.Id)
                .OrderByDescending(r => r.BookingDate)
                .Select(r => new RecordViewModel
                {
                    Id = r.Id,
                    CarDisplay = $"{r.Car.Brand} {r.Car.Model} ({r.Car.VIN})",
                    BookingDate = r.BookingDate,
                    Comment = r.Comment
                })
                .ToListAsync();
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ValidationException("Пользователь не найден");
            }
            return user;
        }
    }

    public class RecordInputModel
    {
        [Required(ErrorMessage = "Выберите автомобиль")]
        [Display(Name = "Автомобиль")]
        public Guid SelectedCarId { get; set; }

        [Required(ErrorMessage = "Укажите дату и время")]
        [Display(Name = "Дата и время")]
        [DataType(DataType.DateTime)]
        [DateAfterYesterday(ErrorMessage = "Дата должна быть позже вчерашнего дня")]
        public DateTime BookingDate { get; set; } = DateTime.Now.AddHours(1);

        [StringLength(500, ErrorMessage = "Комментарий не должен превышать 500 символов")]
        [Display(Name = "Комментарий")]
        public string? Comment { get; set; }

        // Список автомобилей для выбора в форме
        public List<SelectListItem> Cars { get; set; } = new();
    }

    /// <summary>
    /// Валидирует, что дата записи в будущем.
    /// </summary>
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime date)
            {
                return date > DateTime.Now
                    ? ValidationResult.Success
                    : new ValidationResult(ErrorMessage ?? "Дата должна быть в будущем");
            }
            return new ValidationResult("Некорректный формат даты");
        }
    }
}
