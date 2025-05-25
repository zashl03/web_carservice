using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using web_service.Data;
using web_service.Data.Entities;
using web_service.Data.Identity;

namespace web_service.Areas.Identity.Pages.Account.Manage
{
    [Area("Identity")]
    [Authorize(Roles = "Administrator,Storekeeper")]
    public class StorageLocationModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StorageLocationModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public class InputModel
        {
            public Guid Id { get; set; }

            [Required] public string Address { get; set; }
            [Required] public string Room { get; set; }
            [Required] public string Zone { get; set; }
            [Required] public string Rack { get; set; }
            [Required] public int Shelf { get; set; }
            [Required] public int Cell { get; set; }

            [Required(ErrorMessage = "Кладовщик обязателен")]
            public string StorekeeperId { get; set; }  // UserId из EmployeeProfile (string)
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public List<StorageLocationEntity> Locations { get; set; }
        public List<SelectListItem> StorekeeperOptions { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
            await PopulateStorekeeperOptionsAsync();
        }

        public async Task<JsonResult> OnGetLocationAsync(Guid id)
        {
            var loc = await _context.StorageLocations
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(l => l.Id == id);
            if (loc == null)
                return new JsonResult(new { error = "Not found" });

            return new JsonResult(new
            {
                id = loc.Id,
                address = loc.Address,
                room = loc.Room,
                zone = loc.Zone,
                rack = loc.Rack,
                shelf = loc.Shelf,
                cell = loc.Cell,
                storekeeperId = loc.StorekeeperId
            });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine($"[DEBUG] Incoming StorekeeperId = '{Input.StorekeeperId}'");

            if (!ModelState.IsValid)
            {
                foreach (var e in ModelState.Values.SelectMany(v => v.Errors))
                    Console.WriteLine($"Validation error: {e.ErrorMessage}");

                await LoadDataAsync();
                await PopulateStorekeeperOptionsAsync();
                return Page();
            }

            // Проверяем, что профиль кладовщика существует
            var profileExists = await _context.EmployeeProfiles
                .AsNoTracking()
                .AnyAsync(ep => ep.UserId == Input.StorekeeperId);
            if (!profileExists)
            {
                ModelState.AddModelError(nameof(Input.StorekeeperId), "Профиль кладовщика не найден");
                await LoadDataAsync();
                await PopulateStorekeeperOptionsAsync();
                return Page();
            }

            var numberPlace = $"{Input.Address}-{Input.Room}-{Input.Zone}-{Input.Rack}-{Input.Shelf}-{Input.Cell}";

            if (Input.Id == Guid.Empty)
            {
                var newEntity = new StorageLocationEntity
                {
                    Id = Guid.NewGuid(),
                    Address = Input.Address,
                    Room = Input.Room,
                    Zone = Input.Zone,
                    Rack = Input.Rack,
                    Shelf = Input.Shelf,
                    Cell = Input.Cell,
                    NumberPlace = numberPlace,
                    StorekeeperId = Input.StorekeeperId
                };
                _context.StorageLocations.Add(newEntity);
            }
            else
            {
                var entity = await _context.StorageLocations.FindAsync(Input.Id);
                if (entity == null) return NotFound();

                entity.Address = Input.Address;
                entity.Room = Input.Room;
                entity.Zone = Input.Zone;
                entity.Rack = Input.Rack;
                entity.Shelf = Input.Shelf;
                entity.Cell = Input.Cell;
                entity.NumberPlace = numberPlace;
                entity.StorekeeperId = Input.StorekeeperId;

                _context.StorageLocations.Update(entity);
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            ModelState.Clear();
            var entity = await _context.StorageLocations.FindAsync(id);
            if (entity != null)
            {
                _context.StorageLocations.Remove(entity);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        private async Task LoadDataAsync()
        {
            Locations = await _context.StorageLocations
                                      .Include(l => l.Storekeeper)   // EmployeeProfile
                                      .ThenInclude(ep => ep.User)
                                      .OrderBy(l => l.Address)
                                      .ThenBy(l => l.Room)
                                      .ToListAsync();
        }

        private async Task PopulateStorekeeperOptionsAsync()
        {
            // Получаем всех пользователей в роли Storekeeper
            var storekeepers = await _userManager.GetUsersInRoleAsync("Storekeeper");

            // Затем выбираем их профили
            var profiles = await _context.EmployeeProfiles
                .Include(ep => ep.User)
                .Where(ep => storekeepers.Select(u => u.Id).Contains(ep.UserId))
                .ToListAsync();

            StorekeeperOptions = profiles
                .Select(ep => new SelectListItem
                {
                    Value = ep.UserId,
                    Text = $"{ep.User.Email} ({ep.TabNumber})"
                })
                .ToList();

            if (!string.IsNullOrEmpty(Input?.StorekeeperId))
            {
                var sel = StorekeeperOptions.FirstOrDefault(x => x.Value == Input.StorekeeperId);
                if (sel != null) sel.Selected = true;
            }
        }
    }
}
