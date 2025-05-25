using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using web_service.Data;
using web_service.Data.Entities;

namespace web_service.Areas.Identity.Pages.Account.Manage
{
    [Area("Identity")]
    [Authorize(Roles = "Administrator,Storekeeper")]
    public class CategoryPartModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CategoryPartModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<CategoryPartEntity> Categories { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public Guid Id { get; set; }

            [Required]
            [MaxLength(100)]
            public string CategoryName { get; set; }

            [MaxLength(50)]
            public string ShortName { get; set; }

            public Guid? ParentId { get; set; }
        }

        public async Task OnGetAsync()
        {
            Categories = await _context.CategoryParts
                .Include(c => c.Children)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                foreach (var e in ModelState.Values.SelectMany(v => v.Errors))
                    Console.WriteLine($"Validation error: {e.ErrorMessage}");
                await OnGetAsync();
                return Page();
            }

            try
            {
                if (Input.Id == Guid.Empty)
                {
                    var entity = new CategoryPartEntity
                    {
                        Id = Guid.NewGuid(),
                        CategoryName = Input.CategoryName,
                        ShortName = Input.ShortName,
                        ParentId = Input.ParentId
                    };
                    _context.CategoryParts.Add(entity);
                }
                else
                {
                    var entity = await _context.CategoryParts.FindAsync(Input.Id);
                    if (entity == null)
                    {
                        ModelState.AddModelError(string.Empty, "Категория не найдена.");
                        await OnGetAsync();
                        return Page();
                    }

                    entity.CategoryName = Input.CategoryName;
                    entity.ShortName = Input.ShortName;
                    entity.ParentId = Input.ParentId;

                    _context.CategoryParts.Update(entity);
                }

                await _context.SaveChangesAsync();
                return RedirectToPage("./CategoryPart");
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"[ERROR] DB update failed: {dbEx.InnerException?.Message ?? dbEx.Message}");
                ModelState.AddModelError(string.Empty, "Ошибка сохранения в базу данных. Проверьте уникальность названия категории на одном уровне.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] General exception: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Произошла непредвиденная ошибка.");
            }

            await OnGetAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var category = await _context.CategoryParts
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                ModelState.AddModelError(string.Empty, "Категория не найдена");
                return RedirectToPage();
            }

            if (category.Children.Any())
            {
                ModelState.AddModelError(string.Empty, "Сначала удалите подкатегории");
                return RedirectToPage();
            }

            _context.CategoryParts.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
