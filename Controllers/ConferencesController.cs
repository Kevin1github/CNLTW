using ConferenceDelegateManagement1234122.Data;
using ConferenceDelegateManagement1234122.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConferenceDelegateManagement1234122.Controllers
{
    [Authorize]
    public class ConferencesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ConferencesController> _logger;

        public ConferencesController(ApplicationDbContext context, ILogger<ConferencesController> logger)
        {
            _context = context;
            _logger = logger;
        }



        // GET: Conferences
        public async Task<IActionResult> Index(string searchString, string sortOrder, string currentFilter, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var conferences = from c in _context.Conferences
                              select c;

            if (!String.IsNullOrEmpty(searchString))
            {
                conferences = conferences.Where(c => c.Name.Contains(searchString) ||
                                                    c.Description.Contains(searchString) ||
                                                    c.Location.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    conferences = conferences.OrderByDescending(c => c.Name);
                    break;
                case "Date":
                    conferences = conferences.OrderBy(c => c.StartDate);
                    break;
                case "date_desc":
                    conferences = conferences.OrderByDescending(c => c.StartDate);
                    break;
                default:
                    conferences = conferences.OrderBy(c => c.Name);
                    break;
            }

            int pageSize = 10;
            return View(await PaginatedList<Conference>.CreateAsync(conferences.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Conferences/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conference = await _context.Conferences
                .Include(c => c.Sessions)
                .Include(c => c.Registrations)
                    .ThenInclude(r => r.Delegate)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (conference == null)
            {
                return NotFound();
            }

            return View(conference);
        }

        // GET: Conferences/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Conferences/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Name,Description,StartDate,EndDate,Location,MaximumDelegates,RegistrationDeadline,OrganizerEmail,OrganizerPhone,IsActive,Venue")] Conference conference)
        {
            if (ModelState.IsValid)
            {
                _context.Add(conference);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            _logger.LogError("❌ ModelState không hợp lệ.");
            foreach (var error in ModelState)
            {
                foreach (var subError in error.Value.Errors)
                {
                    _logger.LogError($"Lỗi ModelState: {subError.ErrorMessage}");
                }
            }
            return View(conference);
        }

        // GET: Conferences/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conference = await _context.Conferences.FindAsync(id);
            if (conference == null)
            {
                return NotFound();
            }
            return View(conference);
        }

        // POST: Conferences/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,StartDate,EndDate,Location,MaximumDelegates,RegistrationDeadline,OrganizerEmail,OrganizerPhone,IsActive")] Conference conference)
        {
            if (id != conference.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(conference);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConferenceExists(conference.Id))
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
            return View(conference);
        }

        // GET: Conferences/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conference = await _context.Conferences
                .FirstOrDefaultAsync(m => m.Id == id);
            if (conference == null)
            {
                return NotFound();
            }

            return View(conference);
        }

        // POST: Conferences/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var conference = await _context.Conferences.FindAsync(id);
            if (conference == null)
            {
                return NotFound();
            }

            _context.Conferences.Remove(conference);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ConferenceExists(int id)
        {
            return _context.Conferences.Any(e => e.Id == id);
        }
    }

    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}