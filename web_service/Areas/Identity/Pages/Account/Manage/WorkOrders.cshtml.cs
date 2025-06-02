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

namespace web_service.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles = "Administrator")]  // Доступ только для администраторов
    public class WorkOrdersModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public WorkOrdersModel(ApplicationDbContext db)
        {
            _db = db;
        }

        // Для отображения списка заказ-нарядов
        public IList<WorkOrderViewModel> WorkOrdersList { get; set; } = new List<WorkOrderViewModel>();

        // Для выпадающего списка записей (Record) со статусом Approved
        public SelectList ApprovedRecordsSelectList { get; set; } = null!;

        // Модель для привязки данных из формы создания
        [BindProperty]
        public InputModel Input { get; set; } = new();

        // GET: загружаем все заказ-наряды и список отобранных записей
        public async Task<IActionResult> OnGetAsync()
        {
            await PopulateApprovedRecordsAsync();
            await PopulateWorkOrdersListAsync();
            return Page();
        }

        // POST: Создание нового заказ-наряда
        public async Task<IActionResult> OnPostCreateAsync()
        {
            await PopulateApprovedRecordsAsync();
            await PopulateWorkOrdersListAsync();

            if (Input.RecordId == Guid.Empty)
                ModelState.AddModelError(nameof(Input.RecordId), "Нужно выбрать заявку.");

            if (!ModelState.IsValid)
                return Page();

            // Генерируем номер заказ-наряда (например, GUID без дефисов)
            string newWorkOrderNumber = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();

            var newOrder = new WorkOrderEntity
            {
                Id = Guid.NewGuid(),
                WorkOrderNumber = newWorkOrderNumber,
                Cost = null,                     // пока пустая
                DateOpened = DateTime.UtcNow,
                Status = "New",                  // начальный статус
                RecordId = Input.RecordId
            };

            _db.WorkOrders.Add(newOrder);

            var relatedRecord = await _db.Records.FindAsync(Input.RecordId);
            if (relatedRecord != null)
            {
                relatedRecord.Status = "Registered";
                _db.Records.Update(relatedRecord);
            }
            await _db.SaveChangesAsync();

            return RedirectToPage();
        }

        // ==== Вспомогательные методы ====

        // Заполняем ApprovedRecordsSelectList записями RecordEntity со статусом "Approved"
        private async Task PopulateApprovedRecordsAsync()
        {
            // Предполагаем, что у RecordEntity есть поле Status, и статус "Approved" означает, что клиент
            // подтвердил. Если поле называется иначе, исправьте.
            var approvedRecs = await _db.Records
                                        .Include(r => r.Car) // чтобы получить LicensePlate
                                        .Where(r => r.Status == "Approved")
                                        .OrderBy(r => r.DateAppointment)
                                        .ToListAsync();

            // Формируем список: Value = r.Id, Text = "{LicensePlate} — {DateAppointment:yyyy-MM-dd}"
            var items = approvedRecs.Select(r => new
            {
                Id = r.Id,
                Text = $"{r.Car.LicencePlate} — {r.DateAppointment:yyyy-MM-dd}"
            }).ToList();

            ApprovedRecordsSelectList = new SelectList(items, "Id", "Text");
        }

        // Заполняем WorkOrdersList для вывода в таблице
        private async Task PopulateWorkOrdersListAsync()
        {
            WorkOrdersList = await _db.WorkOrders
                                      .Include(wo => wo.Record)
                                      .ThenInclude(r => r.Car)  // чтобы получить LicencePlate
                                      .OrderByDescending(wo => wo.DateOpened)
                                      .Select(wo => new WorkOrderViewModel
                                      {
                                          Id = wo.Id,
                                          LicensePlate = wo.Record.Car.LicencePlate,
                                          WorkOrderNumber = wo.WorkOrderNumber,
                                          Cost = wo.Cost,
                                          Status = wo.Status
                                      })
                                      .ToListAsync();
        }

        // ==== Модели ====

        public class InputModel
        {
            [Required(ErrorMessage = "Нужно выбрать заявку")]
            [Display(Name = "Заявка (LicensePlate — Дата)")]
            public Guid RecordId { get; set; }
        }

        public class WorkOrderViewModel
        {
            public Guid Id { get; set; }

            [Display(Name = "Автомобиль")]
            public string LicensePlate { get; set; } = null!;

            [Display(Name = "Номер заказ-наряда")]
            public string WorkOrderNumber { get; set; } = null!;

            [Display(Name = "Стоимость")]
            [DisplayFormat(DataFormatString = "{0:N2} ₽")]
            public decimal? Cost { get; set; }

            [Display(Name = "Статус")]
            public string Status { get; set; } = null!;
        }
    }
}
