using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using web_service.Data;
using web_service.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_service.Areas.Identity.Pages.Account.Manage
{
    [Area("Identity")]
    [Authorize(Roles = "Administrator,Storekeeper")]
    public class PartInStorageModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PartInStorageModel(ApplicationDbContext context)
        {
            _context = context;
        }


        // Список всех записей PartInStorage для таблицы
        public IList<PartInStorageEntity> PartInStorageList { get; set; }


        //SelectList для запчастей (PartEntity.Id → PartName)
        public SelectList PartSelectList { get; set; }

        //SelectList для мест хранения (StorageLocationEntity.Id → NumberPlace)
        public SelectList StorageSelectList { get; set; }

        //Модель для формы создания/редактирования
        [BindProperty]
        public PartInStorageInputModel Input { get; set; }

        //Вложенная модель для полей формы
        public class PartInStorageInputModel
        {
            public Guid? Id { get; set; }

            [Required]
            public Guid PartId { get; set; }

            [Required]
            public Guid StorageLocationId { get; set; }

            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть положительным числом")]
            public int Quantity { get; set; }

            [Required, MaxLength(20)]
            public string MeasureUnit { get; set; }
        }

        //GET: Инициализация данных для страницы
        public async Task<IActionResult> OnGetAsync()
        {
            await PopulateSelectListsAsync();
            await PopulatePartInStorageListAsync();
            return Page();
        }

        //POST: Создание новой или обновление существующей записи
        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                // Повторно заполняем списки и возвращаем страницу
                await PopulateSelectListsAsync();
                await PopulatePartInStorageListAsync();
                return Page();
            }

            if (Input.Id == null || Input.Id == Guid.Empty)
            {
                // Создаём новую запись
                var newEntry = new PartInStorageEntity
                {
                    Id = Guid.NewGuid(),
                    PartId = Input.PartId,
                    StorageLocationId = Input.StorageLocationId,
                    Quantity = Input.Quantity,
                    MeasureUnit = Input.MeasureUnit
                };
                _context.PartInStorages.Add(newEntry);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Обновляем существующую
                var existing = await _context.PartInStorages.FindAsync(Input.Id.Value);
                if (existing == null)
                {
                    return NotFound();
                }
                existing.PartId = Input.PartId;
                existing.StorageLocationId = Input.StorageLocationId;
                existing.Quantity = Input.Quantity;
                existing.MeasureUnit = Input.MeasureUnit;

                _context.PartInStorages.Update(existing);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        //POST: Удаление записи по Id
        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var toDelete = await _context.PartInStorages.FindAsync(id);
            if (toDelete != null)
            {
                _context.PartInStorages.Remove(toDelete);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        //AJAX GET: Возвращает JSON-данные конкретной записи (для модального окна редактирования)
        public async Task<JsonResult> OnGetPartInStorageDetailsAsync(Guid id)
        {
            var entry = await _context.PartInStorages
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    id = p.Id,
                    partId = p.PartId,
                    storageLocationId = p.StorageLocationId,
                    quantity = p.Quantity,
                    measureUnit = p.MeasureUnit
                })
                .FirstOrDefaultAsync();

            if (entry == null)
                return new JsonResult(null);

            return new JsonResult(entry);
        }

        //Заполняет PartSelectList и StorageSelectList
        private async Task PopulateSelectListsAsync()
        {
            var parts = await _context.Parts
                                      .OrderBy(p => p.PartName)
                                      .ToListAsync();
            PartSelectList = new SelectList(parts, nameof(PartEntity.Id), nameof(PartEntity.PartName));

            var storages = await _context.StorageLocations
                                         .OrderBy(s => s.NumberPlace)
                                         .ToListAsync();
            StorageSelectList = new SelectList(storages, nameof(StorageLocationEntity.Id), nameof(StorageLocationEntity.NumberPlace));
        }

        //Заполняет PartInStorageList с Include навигационных свойств (Part и StorageLocation)
        private async Task PopulatePartInStorageListAsync()
        {
            PartInStorageList = await _context.PartInStorages
                                              .Include(p => p.Part)
                                              .Include(p => p.StorageLocation)
                                              .OrderBy(p => p.Part.PartName)
                                              .ThenBy(p => p.StorageLocation.NumberPlace)
                                              .ToListAsync();
        }
    }
}
