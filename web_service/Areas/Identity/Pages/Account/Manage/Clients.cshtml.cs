using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using web_service.Data;
using web_service.Data.Entities;
using web_service.Data.Identity;

namespace web_service.Areas.Identity.Pages.Account.Manage
{
    [Area("Identity")]
    [Authorize(Roles = "Administrator")]
    public class ClientsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ClientsModel> _logger;

        public ClientsModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ClientsModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // ========== ������ ��� �������� ������ ������� ==========
        public class CreateClientInputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [Display(Name = "����� ��������")]
            [Phone]
            public string PhoneNumber { get; set; } = string.Empty;

            [Required]
            [Display(Name = "������ ���")]
            public string FullName { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "������")]
            public string Password { get; set; } = string.Empty;
        }

        [BindProperty]
        public CreateClientInputModel CreateInput { get; set; }

        // ========== ������ ��� �������������� ������� ==========
        public class EditClientInputModel
        {
            [Required]
            public string Id { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [Display(Name = "����� ��������")]
            [Phone]
            public string PhoneNumber { get; set; } = string.Empty;

            [Required]
            [Display(Name = "������ ���")]
            public string FullName { get; set; } = string.Empty;
        }

        [BindProperty]
        public EditClientInputModel EditInput { get; set; }

        // ========== ������ �������� ��� ������� ==========
        public IList<ClientViewModel> ClientsList { get; set; } = new List<ClientViewModel>();

        // ViewModel ��� ����������� ������ � �������
        public class ClientViewModel
        {
            public string Id { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public DateTime DateCreated { get; set; }
        }

        // ===== GET: �������� �������� + ���������� �� ������ =====
        // �������� "search" � ������ ��� ������ (�� email / FullName / PhoneNumber)
        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // ��������� ���� �������������, � ������� ���� ClientProfile
            var query = _context.Users
                .Include(u => u.ClientProfile)
                .Where(u => u.ClientProfile != null)
                .AsQueryable();

            if (!string.IsNullOrEmpty(Search))
            {
                var s = Search.ToLower();
                query = query.Where(u =>
                    u.Email.ToLower().Contains(s) ||
                    u.FullName.ToLower().Contains(s) ||
                    (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(s)));
            }

            var users = await query
                .OrderBy(u => u.UserName)
                .ToListAsync();

            ClientsList = users.Select(u => new ClientViewModel
            {
                Id = u.Id,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber ?? "",
                FullName = u.FullName,
                DateCreated = u.ClientProfile.DateCreated
            })
            .ToList();

            return Page();
        }

        // ===== POST: ������� ������ ������� =====
        public async Task<IActionResult> OnPostCreateAsync()
        {
            const string PFX = nameof(CreateInput);
            ModelState.ClearValidationState(PFX);
            foreach (var key in ModelState.Keys.Where(k => k != PFX && !k.StartsWith(PFX + ".")).ToList())
                ModelState.Remove(key);

            if (!TryValidateModel(CreateInput, PFX))
                return await OnGetAsync();

            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            // ���������, ��� �� ��� ������������ � ����� Email
            var existing = await _userManager.FindByEmailAsync(CreateInput.Email);
            if (existing != null)
            {
                ModelState.AddModelError("CreateInput.Email", "������������ � ����� email ��� ����������");
                await OnGetAsync();
                return Page();
            }

            // ������ ������ ApplicationUser
            var user = new ApplicationUser
            {
                UserName = CreateInput.Email,
                Email = CreateInput.Email,
                PhoneNumber = CreateInput.PhoneNumber,
                FullName = CreateInput.FullName,
            };

            var result = await _userManager.CreateAsync(user, CreateInput.Password);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
                await OnGetAsync();
                return Page();
            }

            // ��������� ���� "Client" (��������������, ��� ����� ���� ��� ����)
            await _userManager.AddToRoleAsync(user, "Client");

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _userManager.ConfirmEmailAsync(user, token);

            // ������ ClientProfile
            var profile = new ClientProfile
            {
                UserId = user.Id,
                DateCreated = DateTime.UtcNow
            };
            _context.ClientProfiles.Add(profile);
            await _context.SaveChangesAsync();

            _logger.LogInformation("�������� ����� ������ {UserId}", user.Id);
            return RedirectToPage();
        }

        // ===== GET (AJAX): �������� ������ ����� ��� ������� =====
        public async Task<JsonResult> OnGetClientCarsAsync(string id)
        {
            // id � ��� UserId (������)
            var cars = await _context.Cars
                .Where(c => c.ClientProfileId == id)
                .Select(c => new
                {
                    c.Id,
                    c.LicencePlate,
                    c.Brand,
                    c.Model,
                    c.Year,
                    c.Color,
                    c.Mileage
                })
                .ToListAsync();

            return new JsonResult(cars);
        }

        // ===== POST (AJAX): ������� ������� =====
        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            // id � UserId
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // ������� ������� ���������� �������
            var cars = _context.Cars.Where(c => c.ClientProfileId == id);
            _context.Cars.RemoveRange(cars);

            // ����� ������� ��� �������
            var profile = await _context.ClientProfiles.FindAsync(id);
            if (profile != null) _context.ClientProfiles.Remove(profile);

            // � � ����� ������� ApplicationUser
            var delResult = await _userManager.DeleteAsync(user);
            if (!delResult.Succeeded)
            {
                // ����� ������ ������, ���� �����
                return BadRequest(delResult.Errors);
            }

            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true });
        }

        // ===== POST (AJAX): ������������� ������� =====
        public async Task<IActionResult> OnPostEditAsync()
        {
            const string PFX = nameof(EditInput);
            ModelState.ClearValidationState(PFX);
            foreach (var key in ModelState.Keys.Where(k => k != PFX && !k.StartsWith(PFX + ".")).ToList())
                ModelState.Remove(key);

            if (!TryValidateModel(EditInput, PFX))
                return await OnGetAsync();
            if (!ModelState.IsValid)
            {
                foreach (var e in ModelState.Values.SelectMany(v => v.Errors))
                    Console.WriteLine($"Validation error: {e.ErrorMessage}");
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            var user = await _userManager.FindByIdAsync(EditInput.Id);
            if (user == null) return NotFound();

            // ���������, �� ��������� �� Email � ������ �������������
            var emailOwner = await _userManager.FindByEmailAsync(EditInput.Email);
            if (emailOwner != null && emailOwner.Id != EditInput.Id)
            {
                return BadRequest(new[] { "Email ��� ����� ������ �������������" });
            }

            // ��������� ����
            user.Email = EditInput.Email;
            user.UserName = EditInput.Email;
            user.PhoneNumber = EditInput.PhoneNumber;
            user.FullName = EditInput.FullName;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return BadRequest(updateResult.Errors.Select(e => e.Description));
            }

            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true });
        }
    }
}
