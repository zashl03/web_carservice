using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using web_service.Data;
using web_service.Data.Entities;
using web_service.Data.Identity;
using Microsoft.AspNetCore.Identity;

namespace web_service.Areas.Identity.Pages.Account.Manage
{
    [Area("Identity")]
    [Authorize] // Доступна любому аутентифицированному клиенту
    public class RecordModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RecordModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Список машин клиента (для выпадающего списка)
        public SelectList CarSelectList { get; set; }

        // Список услуг (для выпадающего списка)
        public SelectList ServiceSelectList { get; set; }

        // Список всех заявок этого клиента
        public IList<RecordEntity> MyRecords { get; set; }

        // Модель для формы (создать/редактировать)
        [BindProperty]
        public RecordInputModel Input { get; set; }

        public class RecordInputModel
        {
            // Если Id != null, это редактирование
            public Guid? Id { get; set; }

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Дата приёма")]
            public DateTime DateAppointment { get; set; }

            [MaxLength(500)]
            [Display(Name = "Комментарий клиента")]
            public string? ClientComment { get; set; }

            [Required(ErrorMessage = "Выберите автомобиль")]
            [Display(Name = "Автомобиль")]
            public Guid CarId { get; set; }

            [Required(ErrorMessage = "Выберите услугу")]
            [Display(Name = "Услуга")]
            public Guid TypeServiceId { get; set; }
        }

        // GET: инициализация данных
        public async Task<IActionResult> OnGetAsync()
        {
            await PopulateSelectListsAsync();
            await PopulateMyRecordsAsync();
            return Page();
        }

        // POST: сохранение (создание или редактирование)
        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync();
                await PopulateMyRecordsAsync();
                return Page();
            }

            // Валидация даты — не в прошлом
            if (Input.DateAppointment < DateTime.UtcNow.Date)
            {
                ModelState.AddModelError("Input.DateAppointment", "Дата приёма не может быть раньше сегодняшней.");
                await PopulateSelectListsAsync();
                await PopulateMyRecordsAsync();
                return Page();
            }

            // Получаем текущего пользователя
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            // Проверяем, что выбранный CarId действительно принадлежит этому пользователю
            var car = await _context.Cars
                .FirstOrDefaultAsync(c => c.Id == Input.CarId && c.ClientProfileId == user.Id);

            if (car == null)
            {
                ModelState.AddModelError("Input.CarId", "Неверный автомобиль.");
                await PopulateSelectListsAsync();
                await PopulateMyRecordsAsync();
                return Page();
            }

            // Если Input.Id == null → создаём новую заявку
            if (Input.Id == null || Input.Id == Guid.Empty)
            {
                var newRecord = new RecordEntity
                {
                    Id = Guid.NewGuid(),
                    Status = "New", // статус «Создана»
                    DateAppointment = DateTime.SpecifyKind(Input.DateAppointment, DateTimeKind.Utc),
                    ClientComment = Input.ClientComment,
                    RejectReason = null,
                    CarId = car.Id,
                    AdministratorId = null, // пока не назначен
                    TypeServiceId = Input.TypeServiceId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Records.Add(newRecord);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Редактируем существующую заявку
                var existing = await _context.Records.FindAsync(Input.Id.Value);
                if (existing == null)
                {
                    return NotFound();
                }

                // Редактировать можно только если статус = "New"
                if (existing.Status != "New")
                {
                    ModelState.AddModelError(string.Empty, "Редактировать можно только заявку в статусе «Создана».");
                    await PopulateSelectListsAsync();
                    await PopulateMyRecordsAsync();
                    return Page();
                }

                existing.DateAppointment = DateTime.SpecifyKind(Input.DateAppointment, DateTimeKind.Utc);
                existing.ClientComment = Input.ClientComment;
                existing.CarId = Input.CarId;
                existing.TypeServiceId = Input.TypeServiceId;
                // Статус оставляем «New», AdministratorId и RejectReason не меняем

                _context.Records.Update(existing);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        // POST: удаление заявки (отменить)
        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var record = await _context.Records
                .Include(r => r.Car)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (record != null)
            {
                // Можно удалять только если статус = "New"
                if (record.Status == "New")
                {
                    _context.Records.Remove(record);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToPage();
        }

        // AJAX GET: детали заявки для модального редактирования
        public async Task<JsonResult> OnGetRecordDetailsAsync(Guid id)
        {
            var record = await _context.Records
                .Where(r => r.Id == id)
                .Select(r => new
                {
                    id = r.Id,
                    dateAppointment = r.DateAppointment.ToString("yyyy-MM-dd"),
                    clientComment = r.ClientComment,
                    carId = r.CarId,
                    typeServiceId = r.TypeServiceId
                })
                .FirstOrDefaultAsync();

            if (record == null)
                return new JsonResult(null);

            return new JsonResult(record);
        }

        // Вспомогательный метод: заполняет CarSelectList и ServiceSelectList
        private async Task PopulateSelectListsAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            // Машины клиента
            var myCars = await _context.Cars
                             .Where(c => c.ClientProfileId == user.Id)
                             .OrderBy(c => c.LicencePlate)
                             .ToListAsync();

            CarSelectList = new SelectList(myCars, nameof(CarEntity.Id), nameof(CarEntity.LicencePlate));

            // Услуги
            var services = await _context.TypeServices
                                .OrderBy(s => s.ServiceName)
                                .ToListAsync();

            ServiceSelectList = new SelectList(services, nameof(TypeServiceEntity.Id), nameof(TypeServiceEntity.ServiceName));
        }

        // Вспомогательный метод: заполняет MyRecords
        private async Task PopulateMyRecordsAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            MyRecords = await _context.Records
                .Include(r => r.Car)
                .Include(r => r.TypeService)
                .Where(r => r.Car.ClientProfileId == user.Id)
                .OrderByDescending(r => r.DateAppointment)
                .ToListAsync();
        }
    }
}
