using ConferenceDelegateManagement1234122.Data;
using ConferenceDelegateManagement1234122.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConferenceDelegateManagement1234122.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<UserManagementController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: UserManagement
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var delegates = await _context.Delegates.ToListAsync();

            var viewModel = new List<UserDelegateLinkViewModel>();

            foreach (var user in users)
            {
                var delegateRecord = delegates.FirstOrDefault(d => d.Email == user.Email);
                
                viewModel.Add(new UserDelegateLinkViewModel
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    UserName = user.FullName,
                    DelegateId = delegateRecord?.Id,
                    DelegateName = delegateRecord?.FullName,
                    IsLinked = delegateRecord != null
                });
            }

            return View(viewModel);
        }

        // GET: UserManagement/Link/5
        public async Task<IActionResult> Link(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var delegates = await _context.Delegates.ToListAsync();
            var delegateRecord = delegates.FirstOrDefault(d => d.Email == user.Email);

            var viewModel = new UserDelegateLinkViewModel
            {
                UserId = user.Id,
                UserEmail = user.Email,
                UserName = user.FullName,
                DelegateId = delegateRecord?.Id,
                DelegateName = delegateRecord?.FullName,
                IsLinked = delegateRecord != null,
                AvailableDelegates = delegates
            };

            return View(viewModel);
        }

        // POST: UserManagement/Link/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Link(string id, int delegateId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var delegateRecord = await _context.Delegates.FindAsync(delegateId);
            if (delegateRecord == null)
            {
                return NotFound();
            }

            // Update the delegate's email to match the user's email
            delegateRecord.Email = user.Email;
            _context.Update(delegateRecord);
            await _context.SaveChangesAsync();

            // Add the user to the Delegate role if not already
            if (!await _userManager.IsInRoleAsync(user, "Delegate"))
            {
                await _userManager.AddToRoleAsync(user, "Delegate");
            }

            return RedirectToAction(nameof(Index));
        }
    }

    public class UserDelegateLinkViewModel
    {
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public int? DelegateId { get; set; }
        public string DelegateName { get; set; }
        public bool IsLinked { get; set; }
        public List<Delegate1> AvailableDelegates { get; set; } = new List<Delegate1>();
    }
} 