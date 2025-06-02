using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web_service.Data;
using web_service.Data.Entities;

namespace web_service.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles = "Administrator, Mechanic")]  // страницу видят только администраторы
    public class WorkModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public WorkModel(ApplicationDbContext db)
        {
            _db = db;
        }

        // Для отображения списка в таблице
        public List<WorkEntity> WorksList { get; set; } = new();

        // Модель для передачи данных из форм (создание, редактирование)
        [BindProperty]
        public InputModel Input { get; set; } = new();

        // GET: загрузка всех работ
        public async Task OnGetAsync()
        {
            WorksList = await _db.Works
                                 .OrderBy(w => w.WorkName)
                                 .ToListAsync();
        }

        // POST: создание новой работы
        public async Task<IActionResult> OnPostCreateAsync()
        {
            // При создании Input.Id должен быть пустым
            ModelState.Remove(nameof(Input.Id));

            // Перезагрузим список, чтобы в случае ошибок форма + таблица работ отображались
            await OnGetAsync();

            if (!ModelState.IsValid)
                return Page();

            var newWork = new WorkEntity
            {
                Id = Guid.NewGuid(),
                WorkName = Input.WorkName,
                Description = Input.Description,
                Price = Input.Price,
                Duration = Input.Duration
            };

            _db.Works.Add(newWork);
            await _db.SaveChangesAsync();

            return RedirectToPage();  // после создания – reload
        }

        // AJAX GET: получить детали конкретной работы для редактирования (JSON)
        public async Task<JsonResult> OnGetWorkDetailsAsync(Guid id)
        {
            var work = await _db.Works
                                .AsNoTracking()
                                .FirstOrDefaultAsync(w => w.Id == id);
            if (work == null)
                return new JsonResult(null);

            return new JsonResult(new
            {
                id = work.Id,
                workName = work.WorkName,
                description = work.Description,
                price = work.Price,
                duration = work.Duration
            });
        }

        // POST: сохранить изменения при редактировании
        public async Task<IActionResult> OnPostEditAsync()
        {
            // При редактировании Password и т. д. не используется – но здесь только поля работы,
            // поэтому убираем валидацию Id, если оно вдруг не заполнено:
            if (Input.Id == Guid.Empty)
            {
                ModelState.AddModelError(nameof(Input.Id), "Неверный идентификатор работы.");
            }

            // Перезагрузим список для отображения таблицы в случае ошибки
            await OnGetAsync();

            if (!ModelState.IsValid)
                return Page();

            var existing = await _db.Works.FindAsync(Input.Id);
            if (existing == null)
            {
                ModelState.AddModelError(string.Empty, "Работа не найдена.");
                return Page();
            }

            existing.WorkName = Input.WorkName;
            existing.Description = Input.Description;
            existing.Price = Input.Price;
            existing.Duration = Input.Duration;

            _db.Works.Update(existing);
            await _db.SaveChangesAsync();

            return RedirectToPage();
        }

        // POST: удаление работы
        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var toDelete = await _db.Works.FindAsync(id);
            if (toDelete != null)
            {
                _db.Works.Remove(toDelete);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        // Модель InputModel для привязки данных (создание + редактирование)
        public class InputModel
        {
            // При создании идёт Guid.Empty, при редактировании – заполняется AJAX
            public Guid Id { get; set; }

            [Required, MaxLength(100)]
            [Display(Name = "Название работы")]
            public string WorkName { get; set; } = string.Empty;

            [MaxLength(500)]
            [Display(Name = "Описание")]
            public string? Description { get; set; }

            [Required]
            [Range(0.01, 1000000, ErrorMessage = "Цена должна быть больше 0")]
            [Display(Name = "Цена (₽)")]
            public decimal Price { get; set; }

            [Required]
            [Range(1, 10000, ErrorMessage = "Длительность должна быть положительным числом")]
            [Display(Name = "Длительность (мин)")]
            public int Duration { get; set; }
        }
    }
}
