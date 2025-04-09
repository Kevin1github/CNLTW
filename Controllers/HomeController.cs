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

        public async Task<IActionResult> Index()
        {
            try
            {
                if (_context.Database.CanConnect())
                {
                    // Lấy tất cả hội thảo và include các thông tin liên quan
                    var conferences = await _context.Conferences
                        .Include(c => c.Sessions)
                        .Include(c => c.Registrations)
                        .Where(c => c.IsActive)  // Chỉ lấy các hội thảo đang hoạt động
                        .AsNoTracking()
                        .ToListAsync();

                    // Lọc hội thảo sắp diễn ra
                    var upcomingConferences = conferences
                        .Where(c => c.StartDate >= DateTime.Today)
                        .OrderBy(c => c.StartDate)
                        .ToList();

                    // Đếm số lượng delegates từ bảng Delegates
                    var totalDelegates = await _context.Delegates.CountAsync();

                    // Đếm tổng số sessions
                    var totalSessions = await _context.Sessions.CountAsync();

                    _logger.LogInformation($"Found {conferences.Count} conferences, {upcomingConferences.Count} upcoming, {totalDelegates} delegates, {totalSessions} sessions");

                    // Trả dữ liệu vào ViewData
                    ViewData["TotalDelegates"] = totalDelegates;
                    ViewData["TotalConferences"] = conferences.Count;
                    ViewData["TotalSessions"] = totalSessions;
                    ViewData["UpcomingConferences"] = upcomingConferences;

                    return View(conferences);
                }
                else
                {
                    _logger.LogWarning("Database connection is unavailable.");
                    return View(new List<Conference>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data from the database: {Message}", ex.Message);
                return View(new List<Conference>());
            }
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