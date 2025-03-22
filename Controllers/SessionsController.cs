using ConferenceDelegateManagement1234122.Data;
using ConferenceDelegateManagement1234122.Models;
using ConferenceDelegateManagement1234122.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ConferenceDelegateManagement1234122.Controllers
{
    [Authorize]
    public class SessionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SessionsController> _logger;

        public SessionsController(ApplicationDbContext context, ILogger<SessionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Sessions
        public async Task<IActionResult> Index(int? conferenceId, string searchString, string track, int? pageNumber)
        {
            var sessions = _context.Sessions.Include(s => s.Conference).AsQueryable();

            // Filter by conference if specified
            if (conferenceId.HasValue)
            {
                sessions = sessions.Where(s => s.ConferenceId == conferenceId);
                ViewData["ConferenceId"] = conferenceId;
                ViewData["ConferenceName"] = await _context.Conferences
                    .Where(c => c.Id == conferenceId)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync();
            }

            // Search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                sessions = sessions.Where(s => s.Title.Contains(searchString) ||
                                             s.Description.Contains(searchString) ||
                                             s.Speaker.Contains(searchString));
                ViewData["SearchString"] = searchString;
            }

            // Track filter
            if (!string.IsNullOrEmpty(track))
            {
                sessions = sessions.Where(s => s.Track == track);
                ViewData["TrackFilter"] = track;
            }

            // Get tracks for filter dropdown
            ViewData["Tracks"] = await _context.Sessions
                .Where(s => s.Track != null)
                .Select(s => s.Track)
                .Distinct()
                .ToListAsync();

            int pageSize = 10;
            return View(await PaginatedList<Session>.CreateAsync(
                sessions.OrderBy(s => s.StartTime).AsNoTracking(),
                pageNumber ?? 1,
                pageSize));
        }

        // GET: Sessions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions
                .Include(s => s.Conference)
                .Include(s => s.Registrations)
                    .ThenInclude(r => r.Delegate)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (session == null)
            {
                return NotFound();
            }

            return View(session);
        }

        // GET: Sessions/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(int? conferenceId)
        {
            ViewData["ConferenceId"] = new SelectList(_context.Conferences, "Id", "Name", conferenceId);

            // If conferenceId is provided, pre-fill conference details
            if (conferenceId.HasValue)
            {
                var conference = await _context.Conferences.FindAsync(conferenceId);
                if (conference != null)
                {
                    // Pre-populate with conference start date at 9 AM
                    var startTime = conference.StartDate.Date.AddHours(9);
                    var endTime = startTime.AddHours(1);

                    var session = new Session
                    {
                        ConferenceId = conference.Id,
                        StartTime = startTime,
                        EndTime = endTime
                    };

                    return View(session);
                }
            }

            return View();
        }

        // POST: Sessions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Title,Description,StartTime,EndTime,Speaker,Room,MaximumAttendees,Track,ConferenceId")] Session session)
        {
            if (ModelState.IsValid)
            {
                // Validate session times against conference dates
                var conference = await _context.Conferences.FindAsync(session.ConferenceId);
                if (conference != null)
                {
                    if (session.StartTime < conference.StartDate || session.EndTime > conference.EndDate.AddDays(1))
                    {
                        ModelState.AddModelError(string.Empty, "Session times must be within conference dates.");
                        ViewData["ConferenceId"] = new SelectList(_context.Conferences, "Id", "Name", session.ConferenceId);
                        return View(session);
                    }

                    if (session.EndTime <= session.StartTime)
                    {
                        ModelState.AddModelError(string.Empty, "End time must be after start time.");
                        ViewData["ConferenceId"] = new SelectList(_context.Conferences, "Id", "Name", session.ConferenceId);
                        return View(session);
                    }
                }

                _context.Add(session);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { conferenceId = session.ConferenceId });
            }
            ViewData["ConferenceId"] = new SelectList(_context.Conferences, "Id", "Name", session.ConferenceId);
            return View(session);
        }

        // GET: Sessions/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }
            ViewData["ConferenceId"] = new SelectList(_context.Conferences, "Id", "Name", session.ConferenceId);
            return View(session);
        }

        // POST: Sessions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,StartTime,EndTime,Speaker,Room,MaximumAttendees,Track,ConferenceId")] Session session)
        {
            if (id != session.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Validate session times against conference dates
                var conference = await _context.Conferences.FindAsync(session.ConferenceId);
                if (conference != null)
                {
                    if (session.StartTime < conference.StartDate || session.EndTime > conference.EndDate.AddDays(1))
                    {
                        ModelState.AddModelError(string.Empty, "Session times must be within conference dates.");
                        ViewData["ConferenceId"] = new SelectList(_context.Conferences, "Id", "Name", session.ConferenceId);
                        return View(session);
                    }

                    if (session.EndTime <= session.StartTime)
                    {
                        ModelState.AddModelError(string.Empty, "End time must be after start time.");
                        ViewData["ConferenceId"] = new SelectList(_context.Conferences, "Id", "Name", session.ConferenceId);
                        return View(session);
                    }
                }

                try
                {
                    _context.Update(session);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SessionExists(session.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { conferenceId = session.ConferenceId });
            }
            ViewData["ConferenceId"] = new SelectList(_context.Conferences, "Id", "Name", session.ConferenceId);
            return View(session);
        }

        // GET: Sessions/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions
                .Include(s => s.Conference)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (session == null)
            {
                return NotFound();
            }

            return View(session);
        }

        // POST: Sessions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }

            var conferenceId = session.ConferenceId;
            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { conferenceId });
        }

        private bool SessionExists(int id)
        {
            return _context.Sessions.Any(e => e.Id == id);
        }
    }
}