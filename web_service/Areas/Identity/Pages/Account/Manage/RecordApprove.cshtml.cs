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
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using web_service.Data;
using web_service.Data.Entities;

namespace web_service.Areas.Identity.Pages.Account.Manage
{
    [Area("Identity")]
    [Authorize(Roles = "Administrator")]
    public class RecordApproveModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecordApproveModel> _logger;

        public RecordApproveModel(ApplicationDbContext context, ILogger<RecordApproveModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Модель для создания новой заявки администратором
        public class CreateRecordInputModel
        {
            [Required]
            [Display(Name = "Клиент")]
            public string ClientId { get; set; } = string.Empty;  // string, не Guid

            [Required]
            [Display(Name = "Автомобиль")]
            public Guid CarId { get; set; }

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Дата приёма")]
            public DateTime DateAppointment { get; set; }

            [MaxLength(500)]
            [Display(Name = "Комментарий клиента")]
            public string? ClientComment { get; set; }

            [Required]
            [Display(Name = "Услуга")]
            public Guid TypeServiceId { get; set; }

            [Required]
            [MaxLength(50)]
            [Display(Name = "Статус")]
            public string Status { get; set; } = "Approved";
        }

        [BindProperty]
        public CreateRecordInputModel CreateInput { get; set; }

        // Списки для выпадающих списков
        public List<SelectListItem> ClientsSelectList { get; set; } = new();
        public List<SelectListItem> ServicesSelectList { get; set; } = new();
        public List<SelectListItem> StatusSelectList { get; set; } = new();

        // Существующие заявки (для таблицы)
        public IList<RecordEntity> Records { get; set; } = new List<RecordEntity>();

        // Модель для редактирования заявки
        public class RecordApproveInputModel
        {
            public Guid Id { get; set; }

            [MaxLength(500)]
            public string? ClientComment { get; set; }

            [MaxLength(500)]
            public string? RejectReason { get; set; }

            [Required]
            [MaxLength(50)]
            public string Status { get; set; }
        }

        [BindProperty]
        public RecordApproveInputModel Input { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // 1) Загружаем существующие заявки
            Records = await _context.Records
                .Include(r => r.Car)
                    .ThenInclude(c => c.Client)
                        .ThenInclude(cp => cp.User)
                .Include(r => r.TypeService)
                .OrderBy(r => r.DateAppointment)
                .ToListAsync();

            // 2) Список клиентов (ClientProfileId — строка)
            ClientsSelectList = await _context.ClientProfiles
                .Include(c => c.User)
                .Select(c => new SelectListItem
                {
                    Value = c.UserId, // строковый ID
                    Text = c.User.PhoneNumber + " — " + c.User.FullName
                })
                .ToListAsync();

            // 3) Список услуг
            ServicesSelectList = await _context.TypeServices
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.ServiceName
                })
                .ToListAsync();

            // 4) Жёстко заданный список статусов
            StatusSelectList = new List<SelectListItem>
            {
                new SelectListItem { Value = "New", Text = "New" },
                new SelectListItem { Value = "Approved", Text = "Approved", Selected = true }
            };

            return Page();
        }

        // AJAX-метод: возвращает JSON-список машин для выбранного клиента
        public async Task<JsonResult> OnGetCarsAsync(string clientId)
        {
            // Логируем в серверную консоль
            _logger.LogInformation($"OnGetCarsAsync called with clientId = {clientId}");

            var cars = await _context.Cars
                .Where(c => c.ClientProfileId == clientId)
                .Select(c => new
                {
                    id = c.Id,
                    display = c.LicencePlate + " — " + c.Brand // или другое свойство марки
                })
                .ToListAsync();

            return new JsonResult(cars);
        }

        // POST: Создание новой заявки
        public async Task<IActionResult> OnPostCreateAsync()
        {
            const string PFX = nameof(CreateInput);
            ModelState.ClearValidationState(PFX);
            foreach (var key in ModelState.Keys.Where(k => k != PFX && !k.StartsWith(PFX + ".")).ToList())
                ModelState.Remove(key);

            if (!TryValidateModel(CreateInput, PFX))
                return await OnGetAsync();

            if (!ModelState.IsValid)
            {
                foreach (var e in ModelState.Values.SelectMany(v => v.Errors))
                    Console.WriteLine($"Validation error: {e.ErrorMessage}");
                await OnGetAsync();
                return Page();
            }

            var newRecord = new RecordEntity
            {
                Id = Guid.NewGuid(),
                CarId = CreateInput.CarId,
                TypeServiceId = CreateInput.TypeServiceId,
                DateAppointment = DateTime.SpecifyKind(CreateInput.DateAppointment, DateTimeKind.Utc),
                ClientComment = CreateInput.ClientComment,
                Status = CreateInput.Status,
                CreatedAt = DateTime.UtcNow
            };

            _context.Records.Add(newRecord);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        // POST: Редактирование существующей заявки
        public async Task<IActionResult> OnPostApproveAsync()
        {
            const string PFX = nameof(Input);
            ModelState.ClearValidationState(PFX);
            foreach (var key in ModelState.Keys.Where(k => k != PFX && !k.StartsWith(PFX + ".")).ToList())
                ModelState.Remove(key);

            if (!TryValidateModel(Input, PFX))
                return await OnGetAsync();

            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var record = await _context.Records.FindAsync(Input.Id);
            if (record == null)
                return NotFound();

            record.ClientComment = Input.ClientComment;
            record.RejectReason = Input.RejectReason;
            record.Status = Input.Status;

            _context.Records.Update(record);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        // AJAX: детали заявки (без изменений)
        public async Task<JsonResult> OnGetRecordDetailsAsync(Guid id)
        {
            var rec = await _context.Records
                .Include(r => r.Car)
                    .ThenInclude(c => c.Client)
                        .ThenInclude(cp => cp.User)
                .Include(r => r.TypeService)
                .Where(r => r.Id == id)
                .Select(r => new
                {
                    id = r.Id,
                    email = r.Car.Client.User.Email,
                    phoneNumber = r.Car.Client.User.PhoneNumber,
                    licensePlate = r.Car.LicencePlate,
                    dateAppointment = r.DateAppointment.ToString("yyyy-MM-dd"),
                    clientComment = r.ClientComment,
                    serviceName = r.TypeService.ServiceName,
                    rejectReason = r.RejectReason,
                    status = r.Status
                })
                .FirstOrDefaultAsync();

            return new JsonResult(rec);
        }
    }
}
