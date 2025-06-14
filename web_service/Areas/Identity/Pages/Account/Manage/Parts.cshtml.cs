using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using web_service.Data;              // подключите ваш namespace с ApplicationDbContext
using web_service.Data.Entities;    // и сущности PartEntity и CategoryPartEntity
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_service.Areas.Identity.Pages.Account.Manage
{
    [Area("Identity")]
    [Authorize(Roles = "Administrator,Storekeeper")] // Страница доступна только авторизованным пользователям
    public class PartsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PartsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Список для отображения в таблице
        public IList<PartEntity> PartsList { get; set; }

        // Для формирования выпадающего списка категорий
        public SelectList CategorySelectList { get; set; }

        // Модель, в которую привязана форма (создание/редактирование)
        [BindProperty]
        public PartInputModel Input { get; set; }

        // Класс для полей формы
        public class PartInputModel
        {
            public Guid? Id { get; set; } // Если null или Guid.Empty => новая запись

            [Required, MaxLength(50)]
            [Display(Name = "Артикул автосервиса")]
            public string ServicePn { get; set; }

            [Required, MaxLength(50)]
            [Display(Name = "Артикул производителя")]
            public string ManufacturerPn { get; set; }

            [Required, MaxLength(100)]
            [Display(Name = "Производитель")]
            public string Manufacturer { get; set; }

            [Required, MaxLength(100)]
            [Display(Name = "Наименование запчасти")]
            public string PartName { get; set; }

            [Display(Name = "Описание")]
            public string Description { get; set; }

            [Required]
            [Column(TypeName = "numeric(10,2)")]
            [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть больше 0")]
            [Display(Name = "Цена")]
            public decimal Price { get; set; }

            [Required, MaxLength(50)]
            [Display(Name = "OEM номер запчасти")]
            public string OEMNumber { get; set; }

            [Required]
            public Guid CategoryId { get; set; }
        }

        // === Обработчик GET: инициализирует все нужные данные ===
        public async Task<IActionResult> OnGetAsync()
        {
            // Всегда заполняем и категории, и список запчастей
            await PopulateCategoriesAsync();
            await PopulatePartsListAsync();
            return Page();
        }

        // === Обработчик POST для добавления/редактирования ===
        public async Task<IActionResult> OnPostSaveAsync()
        {
            // Проверяем, что модель валидна
            if (!ModelState.IsValid)
            {
                // Если есть ошибки валидации, нужно снова заполнить списки перед возвратом Page()
                foreach (var e in ModelState.Values.SelectMany(v => v.Errors))
                    Console.WriteLine($"Validation error: {e.ErrorMessage}");
                await PopulateCategoriesAsync();
                await PopulatePartsListAsync();
                return Page();
            }

            // Если Input.Id пустой или Guid.Empty => создаём новый объект
            if (Input.Id == null || Input.Id == Guid.Empty)
            {
                var newPart = new PartEntity
                {
                    Id = Guid.NewGuid(),
                    ServicePn = Input.ServicePn,
                    ManufacturerPn = Input.ManufacturerPn,
                    Manufacturer = Input.Manufacturer,
                    PartName = Input.PartName,
                    Description = Input.Description,
                    Price = Input.Price,
                    OEMNumber = Input.OEMNumber,
                    CategoryId = Input.CategoryId
                };

                _context.Parts.Add(newPart);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Редактирование существующей
                var existing = await _context.Parts.FindAsync(Input.Id.Value);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.ServicePn = Input.ServicePn;
                existing.ManufacturerPn = Input.ManufacturerPn;
                existing.Manufacturer = Input.Manufacturer;
                existing.PartName = Input.PartName;
                existing.Description = Input.Description;
                existing.Price = Input.Price;
                existing.OEMNumber = Input.OEMNumber;
                existing.CategoryId = Input.CategoryId;

                _context.Parts.Update(existing);
                await _context.SaveChangesAsync();
            }

            // После успешного сохранения делаем редирект на GET, чтобы избежать повторной отправки формы
            return RedirectToPage();
        }

        // === Обработчик POST для удаления ===
        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var toDelete = await _context.Parts.FindAsync(id);
            if (toDelete != null)
            {
                _context.Parts.Remove(toDelete);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        // === AJAX-метод: возвращает JSON-данные конкретной запчасти для редактирования через JS ===
        public async Task<JsonResult> OnGetPartDetailsAsync(Guid id)
        {
            Console.WriteLine($"[DEBUG] OnGetPartDetailsAsync called. id = {id}");
            var part = await _context.Parts
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    id = p.Id,
                    servicePn = p.ServicePn,
                    manufacturerPn = p.ManufacturerPn,
                    manufacturer = p.Manufacturer,
                    partName = p.PartName,
                    description = p.Description,
                    price = p.Price,
                    oEMNumber = p.OEMNumber,
                    categoryId = p.CategoryId
                })
                .FirstOrDefaultAsync();

            if (part == null)
                return new JsonResult(null);

            return new JsonResult(part);
        }

        // === Вспомогательный метод: заполняет CategorySelectList ===
        private async Task PopulateCategoriesAsync()
        {
            var categories = await _context.CategoryParts
                                           .OrderBy(c => c.CategoryName)
                                           .ToListAsync();
            CategorySelectList = new SelectList(categories,
                                               nameof(CategoryPartEntity.Id),
                                               nameof(CategoryPartEntity.CategoryName));
        }

        // === Вспомогательный метод: заполняет PartsList ===
        private async Task PopulatePartsListAsync()
        {
            PartsList = await _context.Parts
                                     .Include(p => p.Category)
                                     .OrderBy(p => p.PartName)
                                     .ToListAsync();
        }
    }
}
