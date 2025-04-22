/*
 * ���������� ��� ��������� �������� ������� ����������:
 * - ������� ��������
 * - �������� "������������������"
 * - ��������� ������
 * ����������� �� �������� Controller, ���������� ���������� ������ ��� �����������.
 */
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using web_service.Models;

namespace web_service.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;  // ���������� ������ ��� ������ �������

        // ����������� � ���������� ����������� ������� ����� DI
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _logger.LogInformation("HomeController initialized");  // ������ �����������
        }

        // ������������ GET-������� �� ������� �������� (/Home/Index)
        // ���������� ����������� ������������� Views/Home/Index.cshtml
        public IActionResult Index()
        {
            _logger.LogDebug("Loading home page");  // ��������������� ���������
            return View();
        }

        // ������������ GET-������� �� �������� ������������������ (/Home/Privacy)
        // ���������� ������������� Views/Home/Privacy.cshtml
        public IActionResult Privacy()
        {
            _logger.LogDebug("Loading privacy page");
            return View();
        }

        // ������������ ��� �������������� ���������� � ����������
        // ������� ��������� ����������� ������� �� �������
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // ������� ������ ������ � ID ������� ��� �����������
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier // ���������� ������������� �������
            };

            _logger.LogError($"Error occurred. Request ID: {errorViewModel.RequestId}");  // ����������� ������
            return View(errorViewModel);  // ���������� ������������� Views/Shared/Error.cshtml
        }
    }
}