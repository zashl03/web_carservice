using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web_service.Data;
using web_service.Data.Entities;
using web_service.Data.Identity;

namespace web_service.Areas.Identity.Pages.Account.Manage
{
    [Area("Identity")]
    [Authorize] // Доступна любому аутентифицированному клиенту и администратору
    public class MyCarsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyCarsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Список машин для отображения
        public IList<CarEntity> MyCars { get; set; } = new List<CarEntity>();

        // Модель для формы создания/редактирования
        [BindProperty]
        public CarInputModel Input { get; set; }

        // Если админ просматривает чужие автомобили, сюда придёт userId клиента
        [BindProperty(SupportsGet = true)]
        public string? ClientId { get; set; }

        public class CarInputModel
        {
            public Guid? Id { get; set; } // null или Guid.Empty => новая машина

            [Required]
            [StringLength(17, MinimumLength = 17, ErrorMessage = "VIN должен состоять ровно из 17 символов.")]
            [RegularExpression(@"[A-HJ-NPR-Z0-9]{17}", ErrorMessage = "VIN может содержать только буквы (кроме I, O, Q) и цифры.")]
            [Display(Name = "VIN")]
            public string VIN { get; set; }

            [Required]
            [StringLength(9, MinimumLength = 7, ErrorMessage = "Номерной знак должен быть длиной от 7 до 9 символов.")]
            [Display(Name = "Номерной знак")]
            public string LicencePlate { get; set; }

            [Required, MaxLength(50)]
            [Display(Name = "Марка")]
            public string Brand { get; set; }

            [Required, MaxLength(50)]
            [Display(Name = "Модель")]
            public string Model { get; set; }

            [Required, MaxLength(50)]
            [Display(Name = "Пробег")]
            public string Mileage { get; set; }

            [Required]
            [Range(1900, 2100, ErrorMessage = "Год должен быть в диапазоне 1900–2100.")]
            [Display(Name = "Год выпуска")]
            public int Year { get; set; }

            [Required, MaxLength(50)]
            [Display(Name = "Цвет")]
            public string Color { get; set; }
        }

        // GET: загрузка страницы
        public async Task<IActionResult> OnGetAsync()
        {
            await PopulateMyCarsAsync();
            return Page();
        }

        // POST: сохранить (создать или редактировать)
        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                await PopulateMyCarsAsync();
                return Page();
            }

            // Вычисляем userId, для которого выполняем операцию (админ может смотреть чужие)
            string userIdToUse;
            if (User.IsInRole("Administrator") && !string.IsNullOrEmpty(ClientId))
            {
                userIdToUse = ClientId!;
            }
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Challenge();
                userIdToUse = currentUser.Id;
            }

            // Получаем профиль клиента по userIdToUse
            var clientProfile = await _context.ClientProfiles
                .FirstOrDefaultAsync(cp => cp.UserId == userIdToUse);
            if (clientProfile == null)
            {
                ModelState.AddModelError("", "Профиль клиента не найден.");
                await PopulateMyCarsAsync();
                return Page();
            }

            // Сколько уже машин у этого клиента?
            var existingCount = await _context.Cars
                .CountAsync(c => c.ClientProfileId == clientProfile.UserId);

            if (Input.Id == null || Input.Id == Guid.Empty)
            {
                // Создаём новую машину
                if (existingCount >= 10)
                {
                    ModelState.AddModelError("", "Нельзя добавить более 10 автомобилей.");
                    await PopulateMyCarsAsync();
                    return Page();
                }

                // Проверяем уникальность VIN и номерного знака среди всех машин
                bool vinExists = await _context.Cars.AnyAsync(c => c.VIN == Input.VIN);
                if (vinExists)
                {
                    ModelState.AddModelError("Input.VIN", "Автомобиль с таким VIN уже существует.");
                    await PopulateMyCarsAsync();
                    return Page();
                }

                bool plateExists = await _context.Cars.AnyAsync(c => c.LicencePlate == Input.LicencePlate);
                if (plateExists)
                {
                    ModelState.AddModelError("Input.LicencePlate", "Автомобиль с таким номерным знаком уже существует.");
                    await PopulateMyCarsAsync();
                    return Page();
                }

                var newCar = new CarEntity
                {
                    Id = Guid.NewGuid(),
                    VIN = Input.VIN,
                    LicencePlate = Input.LicencePlate,
                    Brand = Input.Brand,
                    Model = Input.Model,
                    Mileage = Input.Mileage,
                    Year = Input.Year,
                    Color = Input.Color,
                    ClientProfileId = clientProfile.UserId
                };

                _context.Cars.Add(newCar);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Редактируем существующую машину
                var existingCar = await _context.Cars
                    .FirstOrDefaultAsync(c =>
                        c.Id == Input.Id.Value &&
                        c.ClientProfileId == clientProfile.UserId);

                if (existingCar == null)
                {
                    return NotFound();
                }

                // Проверяем, что VIN и номерной знак уникальны среди других машин
                bool vinExists = await _context.Cars
                    .AnyAsync(c => c.VIN == Input.VIN && c.Id != existingCar.Id);
                if (vinExists)
                {
                    ModelState.AddModelError("Input.VIN", "Другой автомобиль с таким VIN уже существует.");
                    await PopulateMyCarsAsync();
                    return Page();
                }

                bool plateExists = await _context.Cars
                    .AnyAsync(c => c.LicencePlate == Input.LicencePlate && c.Id != existingCar.Id);
                if (plateExists)
                {
                    ModelState.AddModelError("Input.LicencePlate", "Другой автомобиль с таким номерным знаком уже существует.");
                    await PopulateMyCarsAsync();
                    return Page();
                }

                // Обновляем поля
                existingCar.VIN = Input.VIN;
                existingCar.LicencePlate = Input.LicencePlate;
                existingCar.Brand = Input.Brand;
                existingCar.Model = Input.Model;
                existingCar.Mileage = Input.Mileage;
                existingCar.Year = Input.Year;
                existingCar.Color = Input.Color;

                _context.Cars.Update(existingCar);
                await _context.SaveChangesAsync();
            }

            // После сохранения возвращаемся на эту же страницу, сохраняя ClientId в query
            if (User.IsInRole("Administrator") && !string.IsNullOrEmpty(ClientId))
            {
                return RedirectToPage(new { ClientId = ClientId });
            }
            else
            {
                return RedirectToPage();
            }
        }

        // POST: удалить машину
        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            // Вычисляем userId, для которого удаляем (админ может удалять чужие)
            string userIdToUse;
            if (User.IsInRole("Administrator") && !string.IsNullOrEmpty(ClientId))
            {
                userIdToUse = ClientId!;
            }
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Challenge();
                userIdToUse = currentUser.Id;
            }

            // Получаем клиентский профиль
            var clientProfile = await _context.ClientProfiles
                .FirstOrDefaultAsync(cp => cp.UserId == userIdToUse);
            if (clientProfile == null)
                return Challenge();

            // Находим машину, принадлежащую именно этому клиенту
            var car = await _context.Cars
                .FirstOrDefaultAsync(c => c.Id == id && c.ClientProfileId == clientProfile.UserId);

            if (car != null)
            {
                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();
            }

            // После удаления возвращаемся на эту же страницу, сохраняя ClientId
            if (User.IsInRole("Administrator") && !string.IsNullOrEmpty(ClientId))
            {
                return RedirectToPage(new { ClientId = ClientId });
            }
            else
            {
                return RedirectToPage();
            }
        }

        // AJAX GET: получить данные одной машины
        public async Task<JsonResult> OnGetCarDetailsAsync(Guid id)
        {
            var car = await _context.Cars
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    id = c.Id,
                    vin = c.VIN,
                    licencePlate = c.LicencePlate,
                    brand = c.Brand,
                    model = c.Model,
                    mileage = c.Mileage,
                    year = c.Year,
                    color = c.Color
                })
                .FirstOrDefaultAsync();

            if (car == null)
                return new JsonResult(null);

            return new JsonResult(car);
        }

        // Вспомогательный метод: заполняет MyCars (админ или клиент)
        private async Task PopulateMyCarsAsync()
        {
            string userIdToUse;

            if (User.IsInRole("Administrator") && !string.IsNullOrEmpty(ClientId))
            {
                userIdToUse = ClientId!;
            }
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    MyCars = new List<CarEntity>();
                    return;
                }
                userIdToUse = currentUser.Id;
            }

            var clientProfile = await _context.ClientProfiles
                .FirstOrDefaultAsync(cp => cp.UserId == userIdToUse);
            if (clientProfile == null)
            {
                MyCars = new List<CarEntity>();
                return;
            }

            MyCars = await _context.Cars
                .Where(c => c.ClientProfileId == clientProfile.UserId)
                .OrderBy(c => c.LicencePlate)
                .ToListAsync();
        }
    }
}
