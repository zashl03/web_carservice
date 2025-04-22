/*
 * Razor Page ��� ���������� ������������ � ������ �������� ������������.
 * ������������:
 * - �������� ������ ����������� �����������
 * - ���������� ����� ����������� ����� �����
 * - ���������� � �������� ������� (������������ ��� ����������)
 * �������� ������ �������������� �������������
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using web_service.Data;
using web_service.Data.Identity;
using web_service.Domain.Entities;
using web_service.Models.Car;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using web_service.Data.Entities;

namespace web_service.Areas.Identity.Pages.Account.Manage
{
    [Authorize]  // ��������� ����������� ������ ����
    public class MyCarsModel : PageModel
    {
        private readonly ICarService _carService;      // ������ ��� ������ � ������������
        private readonly ApplicationDbContext _context; // �������� �� ��� ������ ��������
        private readonly UserManager<ApplicationUser> _userManager; // �������� �������������
        private readonly IMapper _mapper;              // ��������������� �������

        [BindProperty]                                 // �������� ������ �� ����� POST
        public AddCarViewModel Input { get; set; }     // ������ ������ ��� ���������� ����

        // ������ ����������� ��� ����������� � �������
        public IEnumerable<CarViewModel> Cars { get; set; } = new List<CarViewModel>();

        public MyCarsModel(
            ICarService carService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            // ������������� ������������, ���������� ����� DI
            _carService = carService;
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        // ��������� GET-������� (�������� ��������)
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login"); // ��������, ���� ������������ �� �����������
            }

            // ����� ������� ������� � ������������ ������������
            var clientProfile = await _context.ClientProfiles
                .Include(c => c.Cars)  // ������ �������� �����������
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            // ������������ ������� ��� ������ �����
            if (clientProfile == null)
            {
                clientProfile = new ClientProfile { UserId = user.Id };
                _context.ClientProfiles.Add(clientProfile);
                await _context.SaveChangesAsync();  // ��������� ����� �������
            }

            // ������� Entity -> ViewModel ��� �����������
            Cars = _mapper.Map<IEnumerable<CarViewModel>>(clientProfile.Cars);
            return Page();
        }

        // ��������� POST-������� (���������� ����������)
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();  // ������� ����� � �������� ���������
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                var clientProfile = await _context.ClientProfiles
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);

                var car = _mapper.Map<Car>(Input);  // �������������� DTO -> Domain Model
                await _carService.AddCarAsync(car, clientProfile.UserId);

                TempData["StatusMessage"] = "���������� ��������!"; // ��������� �� ������
                return RedirectToPage();  // ������� POST-REDIRECT-GET
            }
            catch (Exception ex)  // ��������� ������ �� �������
            {
                ModelState.AddModelError("", ex.Message);
                await OnGetAsync();  // ������������ ������ �����������
                return Page();
            }
        }
    }
}