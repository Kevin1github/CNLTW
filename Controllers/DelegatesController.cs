using ConferenceDelegateManagement1234122.Data;
using ConferenceDelegateManagement1234122.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ConferenceDelegateManagement1234122.Controllers
{
    [Authorize]
    public class DelegatesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DelegatesController> _logger;

        public DelegatesController(ApplicationDbContext context, ILogger<DelegatesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Delegates
        public async Task<IActionResult> Index(string searchString, string sortOrder, string currentFilter, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["OrgSortParm"] = sortOrder == "org" ? "org_desc" : "org";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var delegates = from d in _context.Delegates
                            select d;

            if (!String.IsNullOrEmpty(searchString))
            {
                delegates = delegates.Where(d => d.FirstName.Contains(searchString) ||
                                               d.LastName.Contains(searchString) ||
                                               d.Email.Contains(searchString) ||
                                               d.Organization.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    delegates = delegates.OrderByDescending(d => d.LastName).ThenByDescending(d => d.FirstName);
                    break;
                case "org":
                    delegates = delegates.OrderBy(d => d.Organization);
                    break;
                case "org_desc":
                    delegates = delegates.OrderByDescending(d => d.Organization);
                    break;
                default:
                    delegates = delegates.OrderBy(d => d.LastName).ThenBy(d => d.FirstName);
                    break;
            }

            int pageSize = 10;
            return View(await PaginatedList<Models.Delegate1>.CreateAsync(delegates.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Delegates/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @delegate = await _context.Delegates
                .Include(d => d.Registrations)
                    .ThenInclude(r => r.Conference)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@delegate == null)
            {
                return NotFound();
            }

            return View(@delegate);
        }

        // GET: Delegates/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Delegates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,Phone,Organization,JobTitle,Country,DietaryRequirements,SpecialRequirements")] Models.Delegate1 @delegate)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (await _context.Delegates.AnyAsync(d => d.Email == @delegate.Email))
                {
                    ModelState.AddModelError("Email", "A delegate with this email already exists.");
                    return View(@delegate);
                }

                _context.Add(@delegate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(@delegate);
        }

        // GET: Delegates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @delegate = await _context.Delegates.FindAsync(id);
            if (@delegate == null)
            {
                return NotFound();
            }
            return View(@delegate);
        }

        // POST: Delegates/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Email,Phone,Organization,JobTitle,Country,DietaryRequirements,SpecialRequirements")] Models.Delegate1 @delegate)
        {
            if (id != @delegate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if email already exists for another delegate
                    if (await _context.Delegates.AnyAsync(d => d.Email == @delegate.Email && d.Id != @delegate.Id))
                    {
                        ModelState.AddModelError("Email", "A delegate with this email already exists.");
                        return View(@delegate);
                    }

                    _context.Update(@delegate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DelegateExists(@delegate.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(@delegate);
        }

        // GET: Delegates/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @delegate = await _context.Delegates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@delegate == null)
            {
                return NotFound();
            }

            return View(@delegate);
        }

        // POST: Delegates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @delegate = await _context.Delegates.FindAsync(id);
            if (@delegate == null)
            {
                return NotFound();
            }

            _context.Delegates.Remove(@delegate);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DelegateExists(int id)
        {
            return _context.Delegates.Any(e => e.Id == id);
        }
    }
}