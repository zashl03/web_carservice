// файл: Areas/Identity/Pages/Account/Manage/Payment.cshtml.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using web_service.Data;
using web_service.Data.Entities;
using web_service.Data.Identity;

namespace web_service.Areas.Identity.Pages.Account.Manage
{
    [Area("Identity")]
    [Authorize]
    public class PaymentModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Список завершённых заказ-нарядов для выпадающего списка
        public List<SelectListItem> CompletedWorkOrders { get; set; } = new();

        // Выбранный заказ-наряд (Id)
        [BindProperty]
        public Guid? SelectedWorkOrderId { get; set; }

        // Отображаемая строка для выбранного заказ-наряда
        public string? SelectedWorkOrderDisplay { get; set; }

        // Модель для ввода данных об оплате
        [BindProperty]
        public PaymentInputModel Input { get; set; } = new();

        public class PaymentInputModel
        {
            [Required(ErrorMessage = "Укажите способ оплаты")]
            [Display(Name = "Способ оплаты")]
            public string PaymentType { get; set; } = null!;

            [Required(ErrorMessage = "Укажите дату оплаты")]
            [DataType(DataType.Date)]
            [Display(Name = "Дата оплаты")]
            public DateTime DatePayment { get; set; }

            [Required(ErrorMessage = "Укажите сумму")]
            [Range(0.01, 1000000, ErrorMessage = "Сумма должна быть положительным числом")]
            [Display(Name = "Сумма")]
            public decimal FinalCost { get; set; }
        }

        // GET: сначала просто выводим список завершённых заказ-нарядов
        public async Task<IActionResult> OnGetAsync()
        {
            await PopulateCompletedWorkOrdersAsync();
            return Page();
        }

        // POST: выбран заказ-наряд, показать форму оплаты
        public async Task<IActionResult> OnPostOpenAsync()
        {
            await PopulateCompletedWorkOrdersAsync();

            if (SelectedWorkOrderId == null || SelectedWorkOrderId == Guid.Empty)
            {
                ModelState.AddModelError("SelectedWorkOrderId", "Сначала выберите заказ-наряд.");
                return Page();
            }

            // Получить сам заказ-наряд, чтобы отобразить его в шапке формы
            var workOrder = await _context.WorkOrders
                .Include(w => w.Record)
                    // Здесь предполагается, что RecordEntity имеет навигацию к CarEntity
                    // (например, RecordEntity.Car или RecordEntity.CarId → CarEntity)
                    .ThenInclude(r => r.Car) // приведём к dynamic, чтобы зарезолвить навигацию
                .FirstOrDefaultAsync(w => w.Id == SelectedWorkOrderId.Value);

            if (workOrder == null)
            {
                ModelState.AddModelError("", "Выбранный заказ-наряд не найден.");
                return Page();
            }

            // Собираем строку вида: "{Номер заказ-наряда} – {Номер авто} ({Дата открытия})"
            string carNumber = "";
            try
            {
                // Пытаемся извлечь номер авто через Record.Car.LicencePlate
                var record = workOrder.Record;
                var carEntity = record.Car as CarEntity;
                carNumber = carEntity?.LicencePlate ?? "(автомобиль не найден)";
            }
            catch
            {
                carNumber = "(автомобиль не найден)";
            }

            SelectedWorkOrderDisplay = $"{workOrder.WorkOrderNumber} – {carNumber} ({workOrder.DateOpened:yyyy-MM-dd})";

            // Предзаполняем модель оплаты
            Input.DatePayment = DateTime.Today;
            Input.FinalCost = workOrder.Cost ?? 0m;

            return Page();
        }

        // POST: подтвердить оплату
        public async Task<IActionResult> OnPostConfirmAsync()
        {
            // Нужно заново заполнить список, чтобы перерисовать выпадающий список
            await PopulateCompletedWorkOrdersAsync();

            if (SelectedWorkOrderId == null || SelectedWorkOrderId == Guid.Empty)
            {
                ModelState.AddModelError("SelectedWorkOrderId", "Выбранный заказ-наряд некорректен.");
                return Page();
            }

            if (!ModelState.IsValid)
            {
                // Если валидация не прошла, вернуть ту же страницу
                // и показать ошибки
                return Page();
            }

            // Находим текущего пользователя
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            // Находим профиль сотрудника-администратора по userId
            var adminProfile = await _context.EmployeeProfiles
                .FirstOrDefaultAsync(ep => ep.UserId == currentUser.Id);

            if (adminProfile == null)
            {
                ModelState.AddModelError("", "Профиль администратора не найден.");
                return Page();
            }

            // Находим заказ-наряд
            var workOrder = await _context.WorkOrders
                .FirstOrDefaultAsync(w => w.Id == SelectedWorkOrderId.Value);

            if (workOrder == null)
            {
                ModelState.AddModelError("", "Заказ-наряд куда-то исчез.");
                return Page();
            }

            // Создаём запись об оплате
            var payment = new PaymentEntity
            {
                Id = Guid.NewGuid(),
                PaymentType = Input.PaymentType,
                DatePayment = DateTime.SpecifyKind(Input.DatePayment, DateTimeKind.Utc),
                FinalCost = Input.FinalCost,
                WorkOrderId = workOrder.Id,
                AdministratorId = adminProfile.UserId
            };

            _context.Payments.Add(payment);

            // Меняем статус заказ-наряда
            workOrder.Status = "Closed";

            await _context.SaveChangesAsync();

            // После успешной операции можно перенаправить, например, обратно на саму страницу,
            // чтобы сбросить форму и обновить список (уже без только что закрытого заказа).
            return RedirectToPage();
        }

        // Вспомогательный метод: заполняет CompletedWorkOrders
        private async Task PopulateCompletedWorkOrdersAsync()
        {
            // Берём все заказ-наряды со статусом "Completed"
            var list = await _context.WorkOrders
                .Where(w => w.Status == "Completed")
                .Include(w => w.Record)
                    .ThenInclude(r => r.Car) // предположим, Record → Car
                .ToListAsync();

            CompletedWorkOrders.Clear();
            foreach (var w in list)
            {
                string carNumber;
                try
                {
                    var record = w.Record;
                    var carEntity =record.Car as CarEntity;
                    carNumber = carEntity?.LicencePlate ?? "(автомобиль не найден)";
                }
                catch
                {
                    carNumber = "(автомобиль не найден)";
                }

                var text = $"{w.WorkOrderNumber} – {carNumber} ({w.DateOpened:yyyy-MM-dd})";
                CompletedWorkOrders.Add(new SelectListItem(text, w.Id.ToString()));
            }
        }
    }
}
