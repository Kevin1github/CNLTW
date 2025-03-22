using ConferenceDelegateManagement1234122.Data;
using ConferenceDelegateManagement1234122.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ConferenceDelegateManagement1234122.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            List<Conference> conferences = new List<Conference>();
            List<Conference> upcomingConferences = new List<Conference>();
            int totalDelegates = 0;

            try
            {
                if (_context.Database.CanConnect())
                {
                    // Lấy tất cả hội thảo
                    conferences = _context.Conferences
                        .Include(c => c.Sessions)
                        .AsNoTracking()
                        .ToList();

                    // Lọc hội thảo sắp diễn ra
                    upcomingConferences = conferences
                        .Where(c => c.StartDate >= DateTime.Today)
                        .OrderBy(c => c.StartDate)
                        .Take(5)
                        .ToList();

                    // Lấy số lượng Delegates từ bảng Delegates
                    totalDelegates = _context.Delegates.Count();

                    _logger.LogInformation("Retrieved {Count} conferences, {UpcomingCount} upcoming conferences, {DelegateCount} delegates",
                        conferences.Count, upcomingConferences.Count, totalDelegates);
                }
                else
                {
                    _logger.LogWarning("Database connection is unavailable.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conferences from the database.");
            }

            // Trả dữ liệu trực tiếp vào View
            ViewData["TotalDelegates"] = conferences.Sum(c => c.TotalDelegates);
            ViewData["TotalConferences"] = conferences.Count;
            ViewData["TotalSessions"] = conferences.Sum(c => c.Sessions.Count);

            return View(conferences);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}