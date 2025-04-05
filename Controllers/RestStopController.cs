using ConferenceDelegateManagement1234122.Data;
using ConferenceDelegateManagement1234122.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConferenceDelegateManagement1234122.Controllers
{
    [Authorize]
    public class RestStopController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RestStopController> _logger;

        public RestStopController(ApplicationDbContext context, ILogger<RestStopController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: RestStop
        [Authorize(Roles = "Admin,Delegate")]
        public async Task<IActionResult> Index()
        {
            var restStops = await _context.RestStops
                .Include(r => r.Conference)
                .Include(r => r.Bookings)
                    .ThenInclude(b => b.Registration)
                        .ThenInclude(r => r.Delegate)
                .ToListAsync();
            return View(restStops);
        }

        // GET: RestStop/Details/5
        [Authorize(Roles = "Admin,Delegate")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restStop = await _context.RestStops
                .Include(r => r.Conference)
                .Include(r => r.Bookings)
                    .ThenInclude(b => b.Registration)
                        .ThenInclude(r => r.Delegate)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (restStop == null)
            {
                return NotFound();
            }

            // For delegates, only show their own bookings
            if (!User.IsInRole("Admin"))
            {
                var registration = await _context.Registrations
                    .Include(r => r.Delegate)
                    .FirstOrDefaultAsync(r => r.Delegate.Email == User.Identity.Name && r.ConferenceId == restStop.ConferenceId);

                if (registration == null)
                {
                    return RedirectToAction("AccessDenied", "Account", new { message = "You must be registered for this conference to view accommodation details." });
                }

                // Filter bookings to only show the delegate's own bookings
                restStop.Bookings = restStop.Bookings.Where(b => b.RegistrationId == registration.Id).ToList();
            }

            return View(restStop);
        }

        // GET: RestStop/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(int? conferenceId)
        {
            if (conferenceId == null)
            {
                return NotFound();
            }

            var conference = await _context.Conferences.FindAsync(conferenceId);
            if (conference == null)
            {
                return NotFound();
            }

            var restStop = new RestStop
            {
                ConferenceId = conferenceId.Value
            };

            ViewData["Conferences"] = await _context.Conferences
                .Where(c => c.IsActive)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            return View(restStop);
        }

        // POST: RestStop/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Name,Address,PhoneNumber,PricePerNight,AvailableRooms,Description,ConferenceId")] RestStop restStop)
        {
            if (ModelState.IsValid)
            {
                _context.Add(restStop);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Conferences", new { id = restStop.ConferenceId });
            }

            ViewData["Conferences"] = await _context.Conferences
                .Where(c => c.IsActive)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();
            return View(restStop);
        }

        // GET: RestStop/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restStop = await _context.RestStops.FindAsync(id);
            if (restStop == null)
            {
                return NotFound();
            }

            ViewData["Conferences"] = await _context.Conferences
                .Where(c => c.IsActive)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();
            return View(restStop);
        }

        // POST: RestStop/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,PhoneNumber,PricePerNight,AvailableRooms,Description,ConferenceId")] RestStop restStop)
        {
            if (id != restStop.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(restStop);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RestStopExists(restStop.Id))
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

            ViewData["Conferences"] = await _context.Conferences
                .Where(c => c.IsActive)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();
            return View(restStop);
        }

        // GET: RestStop/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restStop = await _context.RestStops
                .Include(r => r.Conference)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (restStop == null)
            {
                return NotFound();
            }

            return View(restStop);
        }

        // POST: RestStop/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var restStop = await _context.RestStops.FindAsync(id);
            if (restStop == null)
            {
                return NotFound();
            }

            _context.RestStops.Remove(restStop);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: RestStop/Book/5
        [Authorize(Roles = "Admin,Delegate")]
        public async Task<IActionResult> Book(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restStop = await _context.RestStops
                .Include(r => r.Conference)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (restStop == null)
            {
                return NotFound();
            }

            // Get the current user's email
            var userEmail = User.Identity.Name;
            Registration? registration = null;

            // For delegates, we need to find their registration
            if (User.IsInRole("Delegate"))
            {
                // First, find the delegate
                var delegateUser = await _context.Delegates
                    .FirstOrDefaultAsync(d => d.Email == userEmail);

                if (delegateUser == null)
                {
                    // If no delegate found by email, try to find by user ID
                    var currentUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == userEmail);
                    
                    if (currentUser != null)
                    {
                        // Try to find a delegate with a similar name
                        var nameParts = currentUser.FullName.Split(' ');
                        if (nameParts.Length >= 2)
                        {
                            delegateUser = await _context.Delegates
                                .FirstOrDefaultAsync(d => 
                                    d.FirstName == nameParts[0] && 
                                    d.LastName == nameParts[nameParts.Length - 1]);
                        }
                    }
                    
                    if (delegateUser == null)
                    {
                        return RedirectToAction("AccessDenied", "Account", new { message = "Delegate profile not found. Please contact an administrator to link your account." });
                    }
                }

                // Then find their registration for this conference
                registration = await _context.Registrations
                    .FirstOrDefaultAsync(r => r.DelegateId == delegateUser.Id && r.ConferenceId == restStop.ConferenceId);

                if (registration == null)
                {
                    return RedirectToAction("AccessDenied", "Account", new { message = "You must be registered for this conference to book accommodation." });
                }
            }

            var booking = new RestStopBooking
            {
                RestStopId = restStop.Id,
                CheckInDate = DateTime.Today,
                CheckOutDate = DateTime.Today.AddDays(1),
                NumberOfRooms = 1
            };

            if (registration != null)
            {
                booking.RegistrationId = registration.Id;
            }

            ViewData["RestStop"] = restStop;
            ViewData["Registration"] = registration;
            return View(booking);
        }

        // POST: RestStop/Book/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Delegate")]
        public async Task<IActionResult> Book(int id, RestStopBooking booking)
        {
            if (id != booking.RestStopId)
            {
                return NotFound();
            }

            var restStop = await _context.RestStops
                .Include(r => r.Conference)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restStop == null)
            {
                return NotFound();
            }

            // Get the current user's email
            var userEmail = User.Identity.Name;
            Registration? registration = null;

            // For delegates, we need to find their registration
            if (User.IsInRole("Delegate"))
            {
                // First, find the delegate
                var delegateUser = await _context.Delegates
                    .FirstOrDefaultAsync(d => d.Email == userEmail);

                if (delegateUser == null)
                {
                    // If no delegate found by email, try to find by user ID
                    var currentUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == userEmail);
                    
                    if (currentUser != null)
                    {
                        // Try to find a delegate with a similar name
                        var nameParts = currentUser.FullName.Split(' ');
                        if (nameParts.Length >= 2)
                        {
                            delegateUser = await _context.Delegates
                                .FirstOrDefaultAsync(d => 
                                    d.FirstName == nameParts[0] && 
                                    d.LastName == nameParts[nameParts.Length - 1]);
                        }
                    }
                    
                    if (delegateUser == null)
                    {
                        return RedirectToAction("AccessDenied", "Account", new { message = "Delegate profile not found. Please contact an administrator to link your account." });
                    }
                }

                // Then find their registration for this conference
                registration = await _context.Registrations
                    .FirstOrDefaultAsync(r => r.DelegateId == delegateUser.Id && r.ConferenceId == restStop.ConferenceId);

                if (registration == null)
                {
                    return RedirectToAction("AccessDenied", "Account", new { message = "You must be registered for this conference to book accommodation." });
                }
            }

            if (ModelState.IsValid)
            {
                if (User.IsInRole("Delegate") && registration != null)
                {
                    booking.RegistrationId = registration.Id;
                }
                
                var totalDays = (int)(booking.CheckOutDate - booking.CheckInDate).TotalDays;
                booking.TotalPrice = restStop.PricePerNight * booking.NumberOfRooms * totalDays;
                booking.PaymentStatus = Models.Enums.PaymentStatus.Pending;
                booking.CreatedAt = DateTime.UtcNow;

                _context.Add(booking);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Conferences", new { id = restStop.ConferenceId });
            }

            ViewData["RestStop"] = restStop;
            ViewData["Registration"] = registration;
            return View(booking);
        }

        private bool RestStopExists(int id)
        {
            return _context.RestStops.Any(e => e.Id == id);
        }
    }
} 