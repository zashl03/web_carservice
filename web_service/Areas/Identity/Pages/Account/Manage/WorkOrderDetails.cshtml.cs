using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web_service.Data;
using web_service.Data.Entities;
using web_service.Data.Identity;

namespace web_service.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class WorkOrderDetailsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userMgr;

        public WorkOrderDetailsModel(ApplicationDbContext db, UserManager<ApplicationUser> userMgr)
        {
            _db = db;
            _userMgr = userMgr;
        }

        // Для каждого запроса
        public List<SelectListItem> AvailableOrders { get; set; } = new();
        [BindProperty] public Guid SelectedOrderId { get; set; }
        public WorkOrderSummaryViewModel CurrentOrder { get; set; }
        [BindProperty] public string NewStatus { get; set; }

        // Роль пользователя (доступно в Razor)
        public bool IsAdmin { get; private set; }
        public bool IsStorekeeper { get; private set; }
        public bool IsMechanic { get; private set; }

        // Данные таблиц
        public List<WorkTaskViewModel> WorkTasks { get; set; } = new();
        public List<PartInWorkViewModel> PartInWorks { get; set; } = new();

        // Списки для селектов
        public List<SelectListItem> WorksSelectList { get; set; } = new();
        public List<SelectListItem> MechanicsSelectList { get; set; } = new();
        public List<SelectListItem> PartsInStockSelectList { get; set; } = new();
        public List<SelectListItem> StorekeepersSelectList { get; set; } = new();
        public List<SelectListItem> WorkTasksSelectList { get; set; } = new();

        // Модальные модели
        [BindProperty] public CreateWorkTaskModel CreateTask { get; set; }
        [BindProperty] public EditWorkTaskModel EditTask { get; set; }
        [BindProperty] public CreatePartModel CreatePart { get; set; }
        [BindProperty] public EditPartModel EditPart { get; set; }

        // === GET: первая отрисовка ===
        public async Task<IActionResult> OnGetAsync()
        {
            await PopulateRolesAndOrdersAsync();

            if (SelectedOrderId == Guid.Empty)
            {
                CurrentOrder = null;
                return Page();
            }

            await LoadOrderDataAsync(SelectedOrderId);
            return Page();
        }

        // === POST: Открыть выбранный ===
        public async Task<IActionResult> OnPostOpenOrderAsync()
        {
            // просто заново GET
            return await OnGetAsync();
        }

        // === POST: Смена статуса ===
        public async Task<IActionResult> OnPostChangeStatusAsync()
        {
            await PopulateRolesAndOrdersAsync();

            if (SelectedOrderId == Guid.Empty)
                return BadRequest();

            var wo = await _db.WorkOrders.FindAsync(SelectedOrderId);
            if (wo == null)
                return NotFound();

            // логика прав как была
            bool allowed = IsAdmin
                || (IsStorekeeper && wo.Status == "Registered" && NewStatus == "PartsReady")
                || (IsMechanic && (
                      (wo.Status == "PartsReady" && NewStatus == "InProgress")
                   || (wo.Status == "InProgress" && NewStatus == "Completed")));

            if (!allowed)
            {
                ModelState.AddModelError(string.Empty, "У вас нет прав на эту операцию.");
                return await OnGetAsync();
            }

            wo.Status = NewStatus;
            _db.WorkOrders.Update(wo);
            await _db.SaveChangesAsync();

            return await OnGetAsync();
        }

        // === AJAX GET for EditTask ===
        public async Task<JsonResult> OnGetGetTaskAsync(Guid id)
        {
            var t = await _db.WorkTasks.AsNoTracking().FirstOrDefaultAsync(wt => wt.Id == id);
            if (t == null) return new JsonResult(null);
            return new JsonResult(new
            {
                id = t.Id,
                workId = t.WorkId,
                quantity = t.Quantity,
                measureUnit = t.MeasureUnit,
                status = t.Status,
                factCost = t.FactCost,
                mechanicId = t.MechanicId
            });
        }

        // === AJAX GET for EditPart ===
        public async Task<JsonResult> OnGetGetPartAsync(Guid id)
        {
            var p = await _db.PartInWorks.AsNoTracking().FirstOrDefaultAsync(pi => pi.Id == id);
            if (p == null) return new JsonResult(null);
            return new JsonResult(new
            {
                id = p.Id,
                workTaskId = p.WorkTaskId,
                partId = p.PartId,
                storekeeperId = p.StorekeeperId,
                quantity = p.Quantity,
                cost = p.Cost
            });
        }

        // === POST CreateTask ===
        public async Task<IActionResult> OnPostCreateTaskAsync()
        {
            await PopulateRolesAndOrdersAsync();
            const string PFX = nameof(CreateTask);
            ModelState.ClearValidationState(PFX);
            foreach (var key in ModelState.Keys.Where(k => k != PFX && !k.StartsWith(PFX + ".")).ToList())
                ModelState.Remove(key);

            if (!TryValidateModel(CreateTask, PFX))
                return await OnGetAsync();

            _db.WorkTasks.Add(new WorkTaskEntity
            {
                Id = Guid.NewGuid(),
                WorkId = CreateTask.WorkId,
                Quantity = CreateTask.Quantity,
                MeasureUnit = CreateTask.MeasureUnit,
                Status = CreateTask.Status,
                FactCost = CreateTask.FactCost,
                MechanicId = string.IsNullOrWhiteSpace(CreateTask.MechanicId) ? null : CreateTask.MechanicId,
                WorkOrderId = SelectedOrderId
            });
            await _db.SaveChangesAsync();
            return await OnGetAsync();
        }

        // === POST EditTask ===
        public async Task<IActionResult> OnPostEditTaskAsync()
        {
            await PopulateRolesAndOrdersAsync();
            const string PFX = nameof(EditTask);
            ModelState.ClearValidationState(PFX);
            foreach (var key in ModelState.Keys.Where(k => k != PFX && !k.StartsWith(PFX + ".")).ToList())
                ModelState.Remove(key);

            if (!TryValidateModel(EditTask, PFX))
                return await OnGetAsync();

            var e = await _db.WorkTasks.FindAsync(EditTask.Id);
            if (e == null)
            {
                ModelState.AddModelError(string.Empty, "Задача не найдена.");
                return await OnGetAsync();
            }
            e.WorkId = EditTask.WorkId;
            e.Quantity = EditTask.Quantity;
            e.MeasureUnit = EditTask.MeasureUnit;
            e.Status = EditTask.Status;
            e.FactCost = EditTask.FactCost;
            e.MechanicId = string.IsNullOrWhiteSpace(EditTask.MechanicId) ? null : EditTask.MechanicId;
            _db.WorkTasks.Update(e);
            await _db.SaveChangesAsync();
            return await OnGetAsync();
        }

        // === POST DeleteTask ===
        public async Task<IActionResult> OnPostDeleteTaskAsync(Guid id)
        {
            await PopulateRolesAndOrdersAsync();
            var e = await _db.WorkTasks.FindAsync(id);
            if (e != null)
            {
                _db.WorkTasks.Remove(e);
                await _db.SaveChangesAsync();
            }
            return await OnGetAsync();
        }

        // === POST CreatePart ===
        public async Task<IActionResult> OnPostCreatePartAsync()
        {
            await PopulateRolesAndOrdersAsync();
            const string PFX = nameof(CreatePart);
            ModelState.ClearValidationState(PFX);
            foreach (var key in ModelState.Keys.Where(k => k != PFX && !k.StartsWith(PFX + ".")).ToList())
                ModelState.Remove(key);

            if (!TryValidateModel(CreatePart, PFX))
                return await OnGetAsync();

            _db.PartInWorks.Add(new PartInWorkEntity
            {
                Id = Guid.NewGuid(),
                WorkTaskId = CreatePart.WorkTaskId,
                PartId = CreatePart.PartId,
                StorekeeperId = string.IsNullOrWhiteSpace(CreatePart.StorekeeperId) ? null : CreatePart.StorekeeperId,
                Quantity = CreatePart.Quantity,
                Cost = CreatePart.Cost
            });
            await _db.SaveChangesAsync();
            return await OnGetAsync();
        }

        // === POST EditPart ===
        public async Task<IActionResult> OnPostEditPartAsync()
        {
            await PopulateRolesAndOrdersAsync();
            const string PFX = nameof(EditPart);
            ModelState.ClearValidationState(PFX);
            foreach (var key in ModelState.Keys.Where(k => k != PFX && !k.StartsWith(PFX + ".")).ToList())
                ModelState.Remove(key);

            if (!TryValidateModel(EditPart, PFX))
                return await OnGetAsync();

            var e = await _db.PartInWorks.FindAsync(EditPart.Id);
            if (e == null)
            {
                ModelState.AddModelError(string.Empty, "Запись не найдена.");
                return await OnGetAsync();
            }
            e.WorkTaskId = EditPart.WorkTaskId;
            e.PartId = EditPart.PartId;
            e.StorekeeperId = string.IsNullOrWhiteSpace(EditPart.StorekeeperId) ? null : EditPart.StorekeeperId;
            e.Quantity = EditPart.Quantity;
            e.Cost = EditPart.Cost;
            _db.PartInWorks.Update(e);
            await _db.SaveChangesAsync();
            return await OnGetAsync();
        }

        // === POST DeletePart ===
        public async Task<IActionResult> OnPostDeletePartAsync(Guid id)
        {
            await PopulateRolesAndOrdersAsync();
            var e = await _db.PartInWorks.FindAsync(id);
            if (e != null)
            {
                _db.PartInWorks.Remove(e);
                await _db.SaveChangesAsync();
            }
            return await OnGetAsync();
        }

        // ============ вспомогательное =============
        private async Task PopulateRolesAndOrdersAsync()
        {
            // роли
            var user = await _userMgr.GetUserAsync(User);
            var roles = user != null ? await _userMgr.GetRolesAsync(user) : new List<string>();
            IsAdmin = roles.Contains("Administrator");
            IsStorekeeper = roles.Contains("Storekeeper");
            IsMechanic = roles.Contains("Mechanic");

            // заказы
            IQueryable<WorkOrderEntity> query = _db.WorkOrders.Include(w => w.Record).ThenInclude(r => r.Car);

            if (!IsAdmin)
            {
                query = IsStorekeeper
                    ? query.Where(w => w.Status == "Registered")
                    : IsMechanic
                        ? query.Where(w => w.Status == "PartsReady" || w.Status == "InProgress" || w.Status == "Completed")
                        : query.Where(_ => false); // без роли — ничего
            }

            var list = await query.OrderByDescending(w => w.DateOpened)
                .Select(w => new
                {
                    w.Id,
                    Display = w.WorkOrderNumber + " — " + w.Record.Car.LicencePlate + " (" + w.DateOpened.ToString("yyyy-MM-dd") + ")"
                })
                .ToListAsync();

            AvailableOrders = list.Select(x => new SelectListItem(x.Display, x.Id.ToString())).ToList();

            if (SelectedOrderId == Guid.Empty && AvailableOrders.Any())
                SelectedOrderId = Guid.Parse(AvailableOrders[0].Value);
        }


        // ====================================
        // === Вспомогательный загрузчик данных
        // ====================================
        private async Task LoadOrderDataAsync(Guid orderId)
        {
            // --- Ваш оригинальный код LoadOrderDataAsync без изменений ---
            var wo = await _db.WorkOrders
                .Include(w => w.Record).ThenInclude(r => r.Car)
                    .ThenInclude(car => car.Client).ThenInclude(c => c.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == orderId);

            if (wo == null)
            {
                CurrentOrder = null;
                return;
            }

            CurrentOrder = new WorkOrderSummaryViewModel
            {
                Id = wo.Id,
                WorkOrderNumber = wo.WorkOrderNumber,
                LicensePlate = wo.Record.Car.LicencePlate,
                ClientPhone = wo.Record.Car.Client?.User?.PhoneNumber ?? "—",
                DateAppointment = wo.Record.DateAppointment,
                Status = wo.Status,
                Cost = wo.Cost
            };
            NewStatus = CurrentOrder.Status;

            // Списки для задач
            WorksSelectList = await _db.Works.OrderBy(w => w.WorkName)
                                                 .Select(w => new SelectListItem(w.WorkName, w.Id.ToString()))
                                                 .ToListAsync();
            var mechanicsUsers = await _userMgr.GetUsersInRoleAsync("Mechanic");
            var mechanicIds = mechanicsUsers.Select(u => u.Id).ToList();
            MechanicsSelectList = await _db.EmployeeProfiles
                                          .Include(ep => ep.User)
                                          .Where(ep => mechanicIds.Contains(ep.UserId))
                                          .Select(ep => new SelectListItem(ep.User.FullName, ep.UserId))
                                          .ToListAsync();

            // Списки для запчастей
            PartsInStockSelectList = await _db.PartInStorages
                                              .Include(pi => pi.Part)
                                              .Where(pi => pi.Quantity > 0)
                                              .Select(pi => new SelectListItem(
                                                  pi.Part.PartName + " (в наличии: " + pi.Quantity + ")",
                                                  pi.PartId.ToString()))
                                              .ToListAsync();
            var storekeeperUsers = await _userMgr.GetUsersInRoleAsync("Storekeeper");
            var storekeeperIds = storekeeperUsers.Select(u => u.Id).ToList();
            StorekeepersSelectList = await _db.EmployeeProfiles
                                               .Include(ep => ep.User)
                                               .Where(ep => storekeeperIds.Contains(ep.UserId))
                                               .Select(ep => new SelectListItem(ep.User.FullName, ep.UserId))
                                               .ToListAsync();

            // Для выпадающего списка запчастей по задачам
            WorkTasksSelectList = await _db.WorkTasks
                                             .Where(t => t.WorkOrderId == orderId)
                                             .Include(t => t.Work)
                                             .Select(t => new SelectListItem(t.Work.WorkName, t.Id.ToString()))
                                             .ToListAsync();

            // Таблицы моделей для отображения
            WorkTasks = await _db.WorkTasks
                            .Include(t => t.Work)
                            .Include(t => t.Mechanic).ThenInclude(m => m.User)
                            .Where(t => t.WorkOrderId == orderId)
                            .Select(t => new WorkTaskViewModel
                            {
                                Id = t.Id,
                                WorkName = t.Work.WorkName,
                                Quantity = t.Quantity,
                                MeasureUnit = t.MeasureUnit,
                                Status = t.Status,
                                FactCost = t.FactCost,
                                MechanicName = t.Mechanic.User.FullName
                            })
                            .ToListAsync();

            PartInWorks = await _db.PartInWorks
                                .Include(pi => pi.Part)
                                .Include(pi => pi.Storekeeper).ThenInclude(sp => sp.User)
                                .Include(pi => pi.WorkTask).ThenInclude(wt => wt.Work)
                                .Where(pi => pi.WorkTask.WorkOrderId == orderId)
                                .Select(pi => new PartInWorkViewModel
                                {
                                    Id = pi.Id,
                                    WorkName = pi.WorkTask.Work.WorkName,
                                    PartName = pi.Part.PartName,
                                    Quantity = pi.Quantity,
                                    Cost = pi.Cost,
                                    StorekeeperName = pi.Storekeeper.User.FullName
                                })
                                .ToListAsync();
        }

        // ================================
        // === Внутренние классы-модели ===
        // ================================
        public class WorkOrderSummaryViewModel
        {
            public Guid Id { get; set; }

            [Display(Name = "Номер")]
            public string WorkOrderNumber { get; set; } = null!;

            [Display(Name = "Автомобиль")]
            public string LicensePlate { get; set; } = null!;

            [Display(Name = "Телефон клиента")]
            public string ClientPhone { get; set; } = null!;

            [Display(Name = "Дата")]
            public DateTime DateAppointment { get; set; }

            [Display(Name = "Статус")]
            public string Status { get; set; } = null!;

            [Display(Name = "Итоговая стоимость")]
            [DisplayFormat(DataFormatString = "{0:N2} ₽")]
            public decimal? Cost { get; set; }
        }

        public class WorkTaskViewModel
        {
            public Guid Id { get; set; }

            [Display(Name = "Работа")]
            public string WorkName { get; set; } = null!;

            [Display(Name = "Кол-во")]
            public int Quantity { get; set; }

            [Display(Name = "Ед. изм.")]
            public string MeasureUnit { get; set; } = null!;

            [Display(Name = "Статус")]
            public string Status { get; set; } = null!;

            [Display(Name = "Факт. стоимость")]
            [DisplayFormat(DataFormatString = "{0:N2} ₽")]
            public decimal? FactCost { get; set; }

            [Display(Name = "Механик")]
            public string MechanicName { get; set; } = null!;
        }

        public class PartInWorkViewModel
        {
            public Guid Id { get; set; }

            [Display(Name = "Работа")]
            public string WorkName { get; set; } = null!;

            [Display(Name = "Запчасть")]
            public string PartName { get; set; } = null!;

            [Display(Name = "Кол-во")]
            public int Quantity { get; set; }

            [Display(Name = "Стоимость")]
            [DisplayFormat(DataFormatString = "{0:N2} ₽")]
            public decimal Cost { get; set; }

            [Display(Name = "Кладовщик")]
            public string StorekeeperName { get; set; } = null!;
        }

        // ================================
        // === Модели для биндинга форм ===
        // ================================
        public class CreateWorkTaskModel
        {
            [Required]
            [Display(Name = "Работа")]
            public Guid WorkId { get; set; }

            [Required]
            [Range(1, int.MaxValue)]
            [Display(Name = "Кол-во")]
            public int Quantity { get; set; }

            [Required]
            [MaxLength(20)]
            [Display(Name = "Ед. изм.")]
            public string MeasureUnit { get; set; } = null!;

            [Required]
            [MaxLength(20)]
            [Display(Name = "Статус")]
            public string Status { get; set; } = null!;

            [Display(Name = "Факт. стоимость")]
            public decimal? FactCost { get; set; }

            [Display(Name = "Механик")]
            public string? MechanicId { get; set; }
        }

        public class EditWorkTaskModel : CreateWorkTaskModel
        {
            [Required]
            public Guid Id { get; set; }
        }

        public class CreatePartModel
        {
            [Required]
            [Display(Name = "Задача")]
            public Guid WorkTaskId { get; set; }

            [Required]
            [Display(Name = "Запчасть")]
            public Guid PartId { get; set; }

            [Display(Name = "Кладовщик")]
            public string? StorekeeperId { get; set; }

            [Required]
            [Range(1, int.MaxValue)]
            [Display(Name = "Кол-во")]
            public int Quantity { get; set; }

            [Required]
            [Display(Name = "Стоимость")]
            public decimal Cost { get; set; }
        }

        public class EditPartModel : CreatePartModel
        {
            [Required]
            public Guid Id { get; set; }
        }
    }
}
