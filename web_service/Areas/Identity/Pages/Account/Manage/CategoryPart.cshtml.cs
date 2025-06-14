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

        // Полный список всех категорий (дерево)
        public List<CategoryPartEntity> Categories { get; set; } = new();

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

        // =====================
        // 1) OnGetAsync – обычный GET, заполняем Categories
        // =====================
        public async Task OnGetAsync()
        {
            await LoadAllCategories();
        }

        // Загружаем всё дерево (включая детей) сразу
        private async Task LoadAllCategories()
        {
            Categories = await _context.CategoryParts
                .Include(c => c.Children)
                .ToListAsync();
        }

        // =====================
        // 2) AJAX-хендлер для подгрузки/обновления дерева (_CategoryTree.cshtml)
        // =====================
        public async Task<PartialViewResult> OnGetRefreshTreePartialAsync()
        {
            await LoadAllCategories();
            // Отдаём паршал, который рендерит UL со всем деревом
            return Partial("_CategoryTree", Categories.Where(c => c.ParentId == null));
        }

        // =====================
        // 3) OnPostAsync (Add/Edit) – обрабатывает и Add, и Edit
        //    Если это AJAX, возвращаем Partial. Если нет – делаем RedirectToPage.
        // =====================
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadAllCategories();
                // Если это не AJAX, возвращаем обычный Page() с ошибками
                if (!IsAjaxRequest())
                    return Page();

                // Если AJAX, обновить список и вернуть паршал с деревом
                await LoadAllCategories();
                return Partial("_CategoryTree", Categories.Where(c => c.ParentId == null));
            }

            try
            {
                if (Input.Id == Guid.Empty)
                {
                    // Добавление новой категории
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
                    // Редактирование существующей
                    var entity = await _context.CategoryParts.FindAsync(Input.Id);
                    if (entity == null)
                    {
                        ModelState.AddModelError(string.Empty, "Категория не найдена.");
                        await LoadAllCategories();
                        if (!IsAjaxRequest())
                            return Page();

                        return Partial("_CategoryTree", Categories.Where(c => c.ParentId == null));
                    }

                    entity.CategoryName = Input.CategoryName;
                    entity.ShortName = Input.ShortName;
                    entity.ParentId = Input.ParentId;

                    _context.CategoryParts.Update(entity);
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                ModelState.AddModelError(string.Empty, "Ошибка сохранения в базу данных. Проверьте уникальность названия категории на одном уровне.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Произошла непредвиденная ошибка.");
            }

            // После сохранения (или ошибок) 
            if (!IsAjaxRequest())
            {
                // Обычная загрузка страницы
                return RedirectToPage("./CategoryPart");
            }
            else
            {
                // AJAX: вернуть обновлённое дерево
                await LoadAllCategories();
                return Partial("_CategoryTree", Categories.Where(c => c.ParentId == null));
            }
        }

        // =====================
        // 4) OnPostDeleteAsync – удаление категории
        //    Если есть дети – ошибка, иначе удаляем. 
        //    Возвращаем Partial (AJAX) или Redirect (обычно)
        // =====================
        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var category = await _context.CategoryParts
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                // Если узел не найден, тоже вернём ошибку
                if (!IsAjaxRequest())
                {
                    ModelState.AddModelError(string.Empty, "Категория не найдена");
                    return RedirectToPage();
                }
                return BadRequest("Категория не найдена");
            }

            if (category.Children.Any())
            {
                // Вместо того чтобы возвращать Partial, возвращаем 400 Bad Request с сообщением
                if (!IsAjaxRequest())
                {
                    ModelState.AddModelError(string.Empty, "Сначала удалите подкатегории");
                    return RedirectToPage();
                }
                return BadRequest("Нельзя удалить: у категории есть подкатегории");
            }

            _context.CategoryParts.Remove(category);
            await _context.SaveChangesAsync();

            if (!IsAjaxRequest())
            {
                return RedirectToPage();
            }
            else
            {
                await LoadAllCategories();
                return Partial("_CategoryTree", Categories.Where(c => c.ParentId == null));
            }
        }



        // =====================
        // Вспомогательный метод: определяем, пришёл ли запрос через AJAX
        // =====================
        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
    }
}
