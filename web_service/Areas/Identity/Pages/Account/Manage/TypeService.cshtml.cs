using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web_service.Data;
using web_service.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace web_service.Areas.Identity.Pages.Account.Manage
{
    [Area("Identity")]
    [Authorize(Roles = "Administrator")]
    public class TypeServiceModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TypeServiceModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Список всех услуг для таблицы
        public IList<TypeServiceEntity> ServicesList { get; set; }

        // Модель для формы (добавление/редактирование)
        [BindProperty]
        public ServiceInputModel Input { get; set; }

        public class ServiceInputModel
        {
            public Guid? Id { get; set; }

            [Required, MaxLength(100)]
            public string ServiceName { get; set; }

            [MaxLength(500)]
            public string Description { get; set; }
        }

        // GET: заполнение списка
        public async Task<IActionResult> OnGetAsync()
        {
            await PopulateServicesListAsync();
            return Page();
        }

        // POST: добавить или редактировать
        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                await PopulateServicesListAsync();
                return Page();
            }

            if (Input.Id == null || Input.Id == Guid.Empty)
            {
                // создаём новую запись
                var newService = new TypeServiceEntity
                {
                    Id = Guid.NewGuid(),
                    ServiceName = Input.ServiceName,
                    Description = Input.Description
                };
                _context.TypeServices.Add(newService);
                await _context.SaveChangesAsync();
            }
            else
            {
                // редактируем существующую
                var existing = await _context.TypeServices.FindAsync(Input.Id.Value);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.ServiceName = Input.ServiceName;
                existing.Description = Input.Description;
                _context.TypeServices.Update(existing);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        // POST: удалить
        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var toDelete = await _context.TypeServices.FindAsync(id);
            if (toDelete != null)
            {
                _context.TypeServices.Remove(toDelete);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        // AJAX GET: детали для редактирования
        public async Task<JsonResult> OnGetServiceDetailsAsync(Guid id)
        {
            var service = await _context.TypeServices
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    id = s.Id,
                    serviceName = s.ServiceName,
                    description = s.Description
                })
                .FirstOrDefaultAsync();

            if (service == null)
                return new JsonResult(null);

            return new JsonResult(service);
        }

        // Вспомогательный метод: заполняет ServicesList
        private async Task PopulateServicesListAsync()
        {
            ServicesList = await _context.TypeServices
                                         .OrderBy(s => s.ServiceName)
                                         .ToListAsync();
        }
    }
}
