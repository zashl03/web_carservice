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
using web_service.Data;
using web_service.Data.Entities;
using web_service.Data.Identity;

namespace web_service.Areas.Identity.Pages.Account.Manage
{
    [Area("Identity")]
    [Authorize] // �������� ������ �������������������� ������� � ��������������
    public class MyCarsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyCarsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ������ ����� ��� �����������
        public IList<CarEntity> MyCars { get; set; } = new List<CarEntity>();

        // ������ ��� ����� ��������/��������������
        [BindProperty]
        public CarInputModel Input { get; set; }

        // ���� ����� ������������� ����� ����������, ���� ����� userId �������
        [BindProperty(SupportsGet = true)]
        public string? ClientId { get; set; }

        public class CarInputModel
        {
            public Guid? Id { get; set; } // null ��� Guid.Empty => ����� ������

            [Required]
            [StringLength(17, MinimumLength = 17, ErrorMessage = "VIN ������ �������� ����� �� 17 ��������.")]
            [RegularExpression(@"[A-HJ-NPR-Z0-9]{17}", ErrorMessage = "VIN ����� ��������� ������ ����� (����� I, O, Q) � �����.")]
            [Display(Name = "VIN")]
            public string VIN { get; set; }

            [Required]
            [StringLength(9, MinimumLength = 7, ErrorMessage = "�������� ���� ������ ���� ������ �� 7 �� 9 ��������.")]
            [Display(Name = "�������� ����")]
            public string LicencePlate { get; set; }

            [Required, MaxLength(50)]
            [Display(Name = "�����")]
            public string Brand { get; set; }

            [Required, MaxLength(50)]
            [Display(Name = "������")]
            public string Model { get; set; }

            [Required, MaxLength(50)]
            [Display(Name = "������")]
            public string Mileage { get; set; }

            [Required]
            [Range(1900, 2100, ErrorMessage = "��� ������ ���� � ��������� 1900�2100.")]
            [Display(Name = "��� �������")]
            public int Year { get; set; }

            [Required, MaxLength(50)]
            [Display(Name = "����")]
            public string Color { get; set; }
        }

        // GET: �������� ��������
        public async Task<IActionResult> OnGetAsync()
        {
            await PopulateMyCarsAsync();
            return Page();
        }

        // POST: ��������� (������� ��� �������������)
        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                await PopulateMyCarsAsync();
                return Page();
            }

            // ��������� userId, ��� �������� ��������� �������� (����� ����� �������� �����)
            string userIdToUse;
            if (User.IsInRole("Administrator") && !string.IsNullOrEmpty(ClientId))
            {
                userIdToUse = ClientId!;
            }
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Challenge();
                userIdToUse = currentUser.Id;
            }

            // �������� ������� ������� �� userIdToUse
            var clientProfile = await _context.ClientProfiles
                .FirstOrDefaultAsync(cp => cp.UserId == userIdToUse);
            if (clientProfile == null)
            {
                ModelState.AddModelError("", "������� ������� �� ������.");
                await PopulateMyCarsAsync();
                return Page();
            }

            // ������� ��� ����� � ����� �������?
            var existingCount = await _context.Cars
                .CountAsync(c => c.ClientProfileId == clientProfile.UserId);

            if (Input.Id == null || Input.Id == Guid.Empty)
            {
                // ������ ����� ������
                if (existingCount >= 10)
                {
                    ModelState.AddModelError("", "������ �������� ����� 10 �����������.");
                    await PopulateMyCarsAsync();
                    return Page();
                }

                // ��������� ������������ VIN � ��������� ����� ����� ���� �����
                bool vinExists = await _context.Cars.AnyAsync(c => c.VIN == Input.VIN);
                if (vinExists)
                {
                    ModelState.AddModelError("Input.VIN", "���������� � ����� VIN ��� ����������.");
                    await PopulateMyCarsAsync();
                    return Page();
                }

                bool plateExists = await _context.Cars.AnyAsync(c => c.LicencePlate == Input.LicencePlate);
                if (plateExists)
                {
                    ModelState.AddModelError("Input.LicencePlate", "���������� � ����� �������� ������ ��� ����������.");
                    await PopulateMyCarsAsync();
                    return Page();
                }

                var newCar = new CarEntity
                {
                    Id = Guid.NewGuid(),
                    VIN = Input.VIN,
                    LicencePlate = Input.LicencePlate,
                    Brand = Input.Brand,
                    Model = Input.Model,
                    Mileage = Input.Mileage,
                    Year = Input.Year,
                    Color = Input.Color,
                    ClientProfileId = clientProfile.UserId
                };

                _context.Cars.Add(newCar);
                await _context.SaveChangesAsync();
            }
            else
            {
                // ����������� ������������ ������
                var existingCar = await _context.Cars
                    .FirstOrDefaultAsync(c =>
                        c.Id == Input.Id.Value &&
                        c.ClientProfileId == clientProfile.UserId);

                if (existingCar == null)
                {
                    return NotFound();
                }

                // ���������, ��� VIN � �������� ���� ��������� ����� ������ �����
                bool vinExists = await _context.Cars
                    .AnyAsync(c => c.VIN == Input.VIN && c.Id != existingCar.Id);
                if (vinExists)
                {
                    ModelState.AddModelError("Input.VIN", "������ ���������� � ����� VIN ��� ����������.");
                    await PopulateMyCarsAsync();
                    return Page();
                }

                bool plateExists = await _context.Cars
                    .AnyAsync(c => c.LicencePlate == Input.LicencePlate && c.Id != existingCar.Id);
                if (plateExists)
                {
                    ModelState.AddModelError("Input.LicencePlate", "������ ���������� � ����� �������� ������ ��� ����������.");
                    await PopulateMyCarsAsync();
                    return Page();
                }

                // ��������� ����
                existingCar.VIN = Input.VIN;
                existingCar.LicencePlate = Input.LicencePlate;
                existingCar.Brand = Input.Brand;
                existingCar.Model = Input.Model;
                existingCar.Mileage = Input.Mileage;
                existingCar.Year = Input.Year;
                existingCar.Color = Input.Color;

                _context.Cars.Update(existingCar);
                await _context.SaveChangesAsync();
            }

            // ����� ���������� ������������ �� ��� �� ��������, �������� ClientId � query
            if (User.IsInRole("Administrator") && !string.IsNullOrEmpty(ClientId))
            {
                return RedirectToPage(new { ClientId = ClientId });
            }
            else
            {
                return RedirectToPage();
            }
        }

        // POST: ������� ������
        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            // ��������� userId, ��� �������� ������� (����� ����� ������� �����)
            string userIdToUse;
            if (User.IsInRole("Administrator") && !string.IsNullOrEmpty(ClientId))
            {
                userIdToUse = ClientId!;
            }
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Challenge();
                userIdToUse = currentUser.Id;
            }

            // �������� ���������� �������
            var clientProfile = await _context.ClientProfiles
                .FirstOrDefaultAsync(cp => cp.UserId == userIdToUse);
            if (clientProfile == null)
                return Challenge();

            // ������� ������, ������������� ������ ����� �������
            var car = await _context.Cars
                .FirstOrDefaultAsync(c => c.Id == id && c.ClientProfileId == clientProfile.UserId);

            if (car != null)
            {
                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();
            }

            // ����� �������� ������������ �� ��� �� ��������, �������� ClientId
            if (User.IsInRole("Administrator") && !string.IsNullOrEmpty(ClientId))
            {
                return RedirectToPage(new { ClientId = ClientId });
            }
            else
            {
                return RedirectToPage();
            }
        }

        // AJAX GET: �������� ������ ����� ������
        public async Task<JsonResult> OnGetCarDetailsAsync(Guid id)
        {
            var car = await _context.Cars
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    id = c.Id,
                    vin = c.VIN,
                    licencePlate = c.LicencePlate,
                    brand = c.Brand,
                    model = c.Model,
                    mileage = c.Mileage,
                    year = c.Year,
                    color = c.Color
                })
                .FirstOrDefaultAsync();

            if (car == null)
                return new JsonResult(null);

            return new JsonResult(car);
        }

        // ��������������� �����: ��������� MyCars (����� ��� ������)
        private async Task PopulateMyCarsAsync()
        {
            string userIdToUse;

            if (User.IsInRole("Administrator") && !string.IsNullOrEmpty(ClientId))
            {
                userIdToUse = ClientId!;
            }
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    MyCars = new List<CarEntity>();
                    return;
                }
                userIdToUse = currentUser.Id;
            }

            var clientProfile = await _context.ClientProfiles
                .FirstOrDefaultAsync(cp => cp.UserId == userIdToUse);
            if (clientProfile == null)
            {
                MyCars = new List<CarEntity>();
                return;
            }

            MyCars = await _context.Cars
                .Where(c => c.ClientProfileId == clientProfile.UserId)
                .OrderBy(c => c.LicencePlate)
                .ToListAsync();
        }
    }
}
