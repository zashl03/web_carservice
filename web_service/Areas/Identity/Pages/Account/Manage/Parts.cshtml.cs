using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using web_service.Data;              // ���������� ��� namespace � ApplicationDbContext
using web_service.Data.Entities;    // � �������� PartEntity � CategoryPartEntity
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_service.Areas.Identity.Pages.Account.Manage
{
    [Area("Identity")]
    [Authorize(Roles = "Administrator,Storekeeper")] // �������� �������� ������ �������������� �������������
    public class PartsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PartsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // ������ ��� ����������� � �������
        public IList<PartEntity> PartsList { get; set; }

        // ��� ������������ ����������� ������ ���������
        public SelectList CategorySelectList { get; set; }

        // ������, � ������� ��������� ����� (��������/��������������)
        [BindProperty]
        public PartInputModel Input { get; set; }

        // ����� ��� ����� �����
        public class PartInputModel
        {
            public Guid? Id { get; set; } // ���� null ��� Guid.Empty => ����� ������

            [Required, MaxLength(50)]
            public string ServicePn { get; set; }

            [Required, MaxLength(50)]
            public string ManufacturerPn { get; set; }

            [Required, MaxLength(100)]
            public string Manufacturer { get; set; }

            [Required, MaxLength(100)]
            public string PartName { get; set; }

            public string Description { get; set; }

            [Required]
            [Column(TypeName = "numeric(10,2)")]
            [Range(0.01, double.MaxValue, ErrorMessage = "���� ������ ���� ������ 0")]
            public decimal Price { get; set; }

            [Required, MaxLength(50)]
            public string OEMNumber { get; set; }

            [Required]
            public Guid CategoryId { get; set; }
        }

        // === ���������� GET: �������������� ��� ������ ������ ===
        public async Task<IActionResult> OnGetAsync()
        {
            // ������ ��������� � ���������, � ������ ���������
            await PopulateCategoriesAsync();
            await PopulatePartsListAsync();
            return Page();
        }

        // === ���������� POST ��� ����������/�������������� ===
        public async Task<IActionResult> OnPostSaveAsync()
        {
            // ���������, ��� ������ �������
            if (!ModelState.IsValid)
            {
                // ���� ���� ������ ���������, ����� ����� ��������� ������ ����� ��������� Page()
                foreach (var e in ModelState.Values.SelectMany(v => v.Errors))
                    Console.WriteLine($"Validation error: {e.ErrorMessage}");
                await PopulateCategoriesAsync();
                await PopulatePartsListAsync();
                return Page();
            }

            // ���� Input.Id ������ ��� Guid.Empty => ������ ����� ������
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
                // �������������� ������������
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

            // ����� ��������� ���������� ������ �������� �� GET, ����� �������� ��������� �������� �����
            return RedirectToPage();
        }

        // === ���������� POST ��� �������� ===
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

        // === AJAX-�����: ���������� JSON-������ ���������� �������� ��� �������������� ����� JS ===
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

        // === ��������������� �����: ��������� CategorySelectList ===
        private async Task PopulateCategoriesAsync()
        {
            var categories = await _context.CategoryParts
                                           .OrderBy(c => c.CategoryName)
                                           .ToListAsync();
            CategorySelectList = new SelectList(categories,
                                               nameof(CategoryPartEntity.Id),
                                               nameof(CategoryPartEntity.CategoryName));
        }

        // === ��������������� �����: ��������� PartsList ===
        private async Task PopulatePartsListAsync()
        {
            PartsList = await _context.Parts
                                     .Include(p => p.Category)
                                     .OrderBy(p => p.PartName)
                                     .ToListAsync();
        }
    }
}
