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
            List<Conference> conferences = new List<Conference>();
            List<Conference> upcomingConferences = new List<Conference>();
            int totalDelegates = 0;

            try
            {
                if (_context.Database.CanConnect())
                {
                    // Lấy tất cả hội thảo và include các thông tin liên quan
                    conferences = await _context.Conferences
                        .Include(c => c.Sessions)
                        .Include(c => c.Registrations)
                        .AsNoTracking()
                        .ToListAsync();

                    // Lọc hội thảo sắp diễn ra
                    upcomingConferences = conferences
                        .Where(c => c.StartDate >= DateTime.Today)
                        .OrderBy(c => c.StartDate)
                        .Take(5)
                        .ToList();

                    // Lấy số lượng Delegates từ bảng AspNetUsers và AspNetUserRoles
                    var delegateRole = await _context.Roles
                        .FirstOrDefaultAsync(r => r.Name == "Delegate");

                    if (delegateRole != null)
                    {
                        totalDelegates = await _context.UserRoles
                            .Where(ur => ur.RoleId == delegateRole.Id)
                            .CountAsync();
                    }

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
                _logger.LogError(ex, "Error retrieving data from the database: {Message}", ex.Message);
            }

            // Trả dữ liệu vào ViewData
            ViewData["TotalDelegates"] = totalDelegates;
            ViewData["TotalConferences"] = conferences.Count;
            ViewData["TotalSessions"] = conferences.Sum(c => c.Sessions?.Count ?? 0);
            ViewData["UpcomingConferences"] = upcomingConferences;

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