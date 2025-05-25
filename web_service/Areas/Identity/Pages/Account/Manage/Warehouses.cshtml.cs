using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using web_service.Data;
using web_service.Data.Entities;
using web_service.Data.Identity;

namespace web_service.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles = "Administrator,Storekeeper")]
    public class WarehousesModel : PageModel
    {
        private readonly IWarehouseService _warehouseService;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public WarehousesModel(
            IWarehouseService warehouseService,
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _warehouseService = warehouseService;
            _db = db;
            _userManager = userManager;
        }

        // Основная ViewModel
        public WarehousesViewModel ViewModel { get; set; } = new();

        // Модель для формы склада
        [BindProperty]
        public WarehouseInputModel NewWarehouse { get; set; } = new();

        // Модель для формы запчасти (валидация отключена)
        [BindProperty]
        [ValidateNever]
        public PartInputModel NewPart { get; set; } = new();

        public class WarehousesViewModel
        {
            public Guid? SelectedWarehouseId { get; set; }
            public IEnumerable<SelectListItem> WarehouseOptions { get; set; } = new List<SelectListItem>();
            public IEnumerable<SelectListItem> StorekeeperOptions { get; set; } = new List<SelectListItem>();
            public List<PartEntity> Parts { get; set; } = new List<PartEntity>();
        }

        public class WarehouseInputModel
        {
            [Required(ErrorMessage = "Адрес обязателен")]
            public string Address { get; set; }

            [Required(ErrorMessage = "Выберите кладовщика")]
            public string StorekeeperId { get; set; }
        }

        public class PartInputModel
        {
            public Guid WarehouseId { get; set; }

            [Required, MaxLength(100)]
            public string Name { get; set; }

            [Required, MaxLength(50)]
            public string PartNumber { get; set; }

            [Required, Range(1, int.MaxValue)]
            public int Quantity { get; set; }

            [Required, Range(0.01, double.MaxValue)]
            public decimal Cost { get; set; }
        }

        // Загрузка страницы
        public async Task OnGetAsync() => await PopulateOptionsAsync();

        // Обработка добавления склада
        public async Task<IActionResult> OnPostAddWarehouseAsync()
        {
            // 1. Полностью сбрасываем состояние NewPart
            NewPart = new PartInputModel();
            ModelState.Clear();

            // 2. Валидируем только NewWarehouse
            if (!TryValidateModel(NewWarehouse, nameof(NewWarehouse)))
            {
                await PopulateOptionsAsync();
                return Page();
            }

            // 3. Сохраняем склад
            var warehouse = new WarehouseEntity
            {
                Address = NewWarehouse.Address,
                StorekeeperId = NewWarehouse.StorekeeperId
            };

            _db.Warehouses.Add(warehouse);
            await _db.SaveChangesAsync();

            return RedirectToPage();
        }

        // Добавление запчасти
        public async Task<IActionResult> OnPostAddPartAsync()
        {
            await PopulateOptionsAsync();

            // Валидируем ТОЛЬКО модель NewPart
            if (!TryValidateModel(NewPart, nameof(NewPart)))
            {
                // Подгружаем части выбранного склада
                if (NewPart.WarehouseId != Guid.Empty)
                {
                    ViewModel.Parts = (await _warehouseService
                    .GetPartsByWarehouseAsync(ViewModel.SelectedWarehouseId.Value))
                    .ToList();
                }
                return Page();
            }

            _db.Parts.Add(new PartEntity
            {
                Name = NewPart.Name,
                PartNumber = NewPart.PartNumber,
                Quantity = NewPart.Quantity,
                Cost = NewPart.Cost,
                WarehouseId = NewPart.WarehouseId
            });

            await _db.SaveChangesAsync();
            return RedirectToPage();
        }

        // Заполнение списков
        private async Task PopulateOptionsAsync()
        {
            // Склады
            ViewModel.WarehouseOptions = (await _warehouseService.GetAllWarehousesAsync())
                .Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = w.Address
                });

            // Кладовщики
            var storekeepers = await _userManager.GetUsersInRoleAsync("Storekeeper");
            ViewModel.StorekeeperOptions = storekeepers
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.Email
                });
        }
    }
}