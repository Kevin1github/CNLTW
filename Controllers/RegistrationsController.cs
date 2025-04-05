using System;
using System.Linq;
using System.Threading.Tasks;
using ConferenceDelegateManagement1234122.Data;
using ConferenceDelegateManagement1234122.Models;
using ConferenceDelegateManagement1234122.Models.Enums;
using ConferenceDelegateManagement1234122.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ConferenceDelegateManagement1234122.Controllers
{
    [Authorize]
    public class RegistrationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RegistrationsController> _logger;

        public RegistrationsController(ApplicationDbContext context, ILogger<RegistrationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register(int conferenceId)
        {
            if (conferenceId <= 0)
            {
                return NotFound();
            }

            var conference = _context.Conferences.Find(conferenceId);
            if (conference == null)
            {
                return NotFound();
            }

            var model = new RegisterViewModel
            {
                ConferenceId = conference.Id,
                ConferenceName = conference.Name,
                RegistrationFee = conference.RegistrationFee,
                PaymentMethod = "Cash"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var conference = await _context.Conferences.FindAsync(model.ConferenceId);
                if (conference == null)
                {
                    return NotFound("Conference not found.");
                }

                var delegateUser = await _context.Delegates.FirstOrDefaultAsync(d => d.Email == User.Identity.Name);
                if (delegateUser == null)
                {
                    return BadRequest("Delegate not found.");
                }

                var registration = new Registration
                {
                    DelegateId = delegateUser.Id,
                    ConferenceId = model.ConferenceId,
                    RegistrationDate = DateTime.Now,
                    PaymentStatus = PaymentStatus.Pending,
                    RegistrationCode = GenerateRegistrationCode()
                };

                _context.Registrations.Add(registration);
                await _context.SaveChangesAsync();

                if (model.PaymentMethod == "Cash")
                {
                    TempData["SuccessMessage"] = "Registration successful! Please pay in cash at the conference.";
                    return RedirectToAction("Index", "Home");
                }
                else if (model.PaymentMethod == "QRCode")
                {
                    TempData["PaymentAmount"] = conference.RegistrationFee.ToString("N0");
                    TempData["PaymentCode"] = registration.RegistrationCode;
                    return RedirectToAction("ShowQRCode", new { registrationId = registration.Id });
                }
            }

            var conf = await _context.Conferences.FindAsync(model.ConferenceId);
            if (conf != null)
            {
                model.ConferenceName = conf.Name;
                model.RegistrationFee = conf.RegistrationFee;
            }
            return View(model);
        }

        private string GenerateRegistrationCode()
        {
            return $"REG-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        public IActionResult ShowQRCode(int registrationId)
        {
            var registration = _context.Registrations
                .Include(r => r.Conference)
                .Include(r => r.Delegate)
                .FirstOrDefault(r => r.Id == registrationId);

            if (registration == null)
            {
                return NotFound();
            }

            // Tạo nội dung QR code (ví dụ: thông tin chuyển khoản)
            var qrContent = $"Conference: {registration.Conference.Name}\n" +
                          $"Amount: {registration.Conference.RegistrationFee:N0} VND\n" +
                          $"Code: {registration.RegistrationCode}";

            ViewData["QRContent"] = qrContent;
            ViewData["Amount"] = registration.Conference.RegistrationFee.ToString("N0");
            ViewData["RegistrationCode"] = registration.RegistrationCode;

            return View();
        }

        // GET: Registrations
        [Authorize(Roles = "Admin")] // Chỉ admin mới xem được danh sách đăng ký
        public async Task<IActionResult> Index(int? conferenceId, int? delegateId, string paymentStatus, int? pageNumber)
        {
            var registrations = _context.Registrations
                .Include(r => r.Conference)
                .Include(r => r.Delegate)
                .AsQueryable();

            // Filter by conference if specified
            if (conferenceId.HasValue)
            {
                registrations = registrations.Where(r => r.ConferenceId == conferenceId);
                ViewData["ConferenceId"] = conferenceId;
                ViewData["ConferenceName"] = await _context.Conferences
                    .Where(c => c.Id == conferenceId)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync();
            }

            // Filter by delegate if specified
            if (delegateId.HasValue)
            {
                registrations = registrations.Where(r => r.DelegateId == delegateId);
                ViewData["DelegateId"] = delegateId;
                var delegateData = await _context.Delegates
                    .Where(d => d.Id == delegateId)
                    .Select(d => new { d.FirstName, d.LastName })
                    .FirstOrDefaultAsync();
                if (delegateData != null)
                {
                    ViewData["DelegateName"] = $"{delegateData.FirstName} {delegateData.LastName}";
                }
            }

            // Filter by payment status if specified
            if (!string.IsNullOrEmpty(paymentStatus) && Enum.TryParse<PaymentStatus>(paymentStatus, out var status))
            {
                registrations = registrations.Where(r => r.PaymentStatus == status);
                ViewData["PaymentStatusFilter"] = paymentStatus;
            }

            // Payment status for filter dropdown
            ViewData["PaymentStatuses"] = Enum.GetNames(typeof(PaymentStatus));

            int pageSize = 10;
            return View(await PaginatedList<Registration>.CreateAsync(
                registrations.OrderByDescending(r => r.RegistrationDate).AsNoTracking(),
                pageNumber ?? 1,
                pageSize));
        }

        // GET: Registrations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registration = await _context.Registrations
                .Include(r => r.Conference)
                .Include(r => r.Delegate)
                .Include(r => r.Sessions)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (registration == null)
            {
                return NotFound();
            }

            return View(registration);
        }

        // GET: Registrations/Create
        public async Task<IActionResult> Create(int? conferenceId, int? delegateId)
        {
            // Prepare dropdowns
            var conferencesQuery = _context.Conferences
                .Where(c => c.IsActive && (c.RegistrationDeadline == null || c.RegistrationDeadline >= DateTime.Today))
                .OrderBy(c => c.Name);

            if (conferenceId.HasValue)
            {
                // If conference is specified but no longer open for registration, still include it
                var specificConf = await _context.Conferences.FindAsync(conferenceId);
                if (specificConf != null && !conferencesQuery.Any(c => c.Id == conferenceId))
                {
                    conferencesQuery = conferencesQuery
                        .Union(_context.Conferences.Where(c => c.Id == conferenceId))
                        .OrderBy(c => c.Id);
                }
            }

            ViewData["ConferenceId"] = new SelectList(await conferencesQuery.ToListAsync(), "Id", "Name", conferenceId);
            ViewData["DelegateId"] = new SelectList(_context.Delegates.OrderBy(d => d.LastName).ThenBy(d => d.FirstName), "Id", "FullName", delegateId);

            // If conference is selected, load available sessions
            if (conferenceId.HasValue)
            {
                ViewData["AvailableSessions"] = await _context.Sessions
                    .Where(s => s.ConferenceId == conferenceId)
                    .OrderBy(s => s.StartTime)
                    .ToListAsync();
            }

            // Pre-populate the model if conference and delegate are specified
            if (conferenceId.HasValue && delegateId.HasValue)
            {
                // Check if registration already exists
                var existingRegistration = await _context.Registrations
                    .FirstOrDefaultAsync(r => r.ConferenceId == conferenceId && r.DelegateId == delegateId);

                if (existingRegistration != null)
                {
                    return RedirectToAction(nameof(Edit), new { id = existingRegistration.Id });
                }

                return View(new Registration
                {
                    ConferenceId = conferenceId.Value,
                    DelegateId = delegateId.Value,
                    RegistrationDate = DateTime.UtcNow
                });
            }

            return View();
        }

        // POST: Registrations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DelegateId,ConferenceId,RegistrationDate,PaymentStatus,AttendanceConfirmed,Notes")] Registration registration, int[]? selectedSessions)
        {
            if (ModelState.IsValid)
            {
                // Check if registration already exists
                var existingRegistration = await _context.Registrations
                    .FirstOrDefaultAsync(r => r.ConferenceId == registration.ConferenceId && r.DelegateId == registration.DelegateId);

                if (existingRegistration != null)
                {
                    ModelState.AddModelError(string.Empty, "A registration for this delegate and conference already exists.");
                    PrepareViewDataForCreate(registration.ConferenceId, registration.DelegateId);
                    return View(registration);
                }

                // Check if conference has reached maximum delegates
                var conference = await _context.Conferences
                    .Include(c => c.Registrations)
                    .FirstOrDefaultAsync(c => c.Id == registration.ConferenceId);

                if (conference != null && conference.MaximumDelegates > 0)
                {
                    if (conference.Registrations.Count >= conference.MaximumDelegates)
                    {
                        ModelState.AddModelError(string.Empty, "This conference has reached its maximum number of delegates.");
                        PrepareViewDataForCreate(registration.ConferenceId, registration.DelegateId);
                        return View(registration);
                    }
                }

                // Check if registration deadline has passed
                if (conference != null && conference.RegistrationDeadline.HasValue && conference.RegistrationDeadline.Value < DateTime.Today)
                {
                    ModelState.AddModelError(string.Empty, "The registration deadline for this conference has passed.");
                    PrepareViewDataForCreate(registration.ConferenceId, registration.DelegateId);
                    return View(registration);
                }

                // Generate a unique registration code
                registration.RegistrationCode = GenerateUniqueRegistrationCode();

                // Add registration to database
                _context.Add(registration);
                await _context.SaveChangesAsync();

                // Add selected sessions if any
                if (selectedSessions != null && selectedSessions.Length > 0)
                {
                    var sessions = await _context.Sessions
                        .Where(s => selectedSessions.Contains(s.Id) && s.ConferenceId == registration.ConferenceId)
                        .ToListAsync();

                    foreach (var session in sessions)
                    {
                        registration.Sessions.Add(session);
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Details), new { id = registration.Id });
            }

            PrepareViewDataForCreate(registration.ConferenceId, registration.DelegateId);
            return View(registration);
        }

        private async void PrepareViewDataForCreate(int? conferenceId, int? delegateId)
        {
            var conferencesQuery = _context.Conferences
                .Where(c => c.IsActive && (c.RegistrationDeadline == null || c.RegistrationDeadline >= DateTime.Today))
                .OrderBy(c => c.Name);

            if (conferenceId.HasValue)
            {
                // If conference is specified but no longer open for registration, still include it
                var specificConf = await _context.Conferences.FindAsync(conferenceId);
                if (specificConf != null && !conferencesQuery.Any(c => c.Id == conferenceId))
                {
                    conferencesQuery = conferencesQuery
                        .Union(_context.Conferences.Where(c => c.Id == conferenceId))
                        .OrderBy(c => c.Id);
                }
            }

            ViewData["ConferenceId"] = new SelectList(await conferencesQuery.ToListAsync(), "Id", "Name", conferenceId);
            ViewData["DelegateId"] = new SelectList(_context.Delegates.OrderBy(d => d.LastName).ThenBy(d => d.FirstName), "Id", "FullName", delegateId);

            if (conferenceId.HasValue)
            {
                ViewData["AvailableSessions"] = await _context.Sessions
                    .Where(s => s.ConferenceId == conferenceId)
                    .OrderBy(s => s.StartTime)
                    .ToListAsync();
            }
        }

        // GET: Registrations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registration = await _context.Registrations
                .Include(r => r.Sessions)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (registration == null)
            {
                return NotFound();
            }

            ViewData["ConferenceId"] = new SelectList(_context.Conferences, "Id", "Name", registration.ConferenceId);
            ViewData["DelegateId"] = new SelectList(_context.Delegates.OrderBy(d => d.LastName).ThenBy(d => d.FirstName), "Id", "FullName", registration.DelegateId);
            ViewData["PaymentStatusList"] = new SelectList(Enum.GetValues(typeof(PaymentStatus)).Cast<PaymentStatus>(), registration.PaymentStatus);

            // Get available sessions for this conference
            ViewData["AvailableSessions"] = await _context.Sessions
                .Where(s => s.ConferenceId == registration.ConferenceId)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            // Get IDs of currently selected sessions
            ViewData["SelectedSessions"] = registration.Sessions.Select(s => s.Id).ToList();

            return View(registration);
        }

        // POST: Registrations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DelegateId,ConferenceId,RegistrationDate,PaymentStatus,AttendanceConfirmed,RegistrationCode,Notes")] Registration registration, int[]? selectedSessions)
        {
            if (id != registration.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get existing registration with sessions to update
                    var existingRegistration = await _context.Registrations
                        .Include(r => r.Sessions)
                        .FirstOrDefaultAsync(r => r.Id == id);

                    if (existingRegistration == null)
                    {
                        return NotFound();
                    }

                    // Update properties
                    existingRegistration.PaymentStatus = registration.PaymentStatus;
                    existingRegistration.AttendanceConfirmed = registration.AttendanceConfirmed;
                    existingRegistration.Notes = registration.Notes;

                    // Update sessions
                    existingRegistration.Sessions.Clear();

                    if (selectedSessions != null && selectedSessions.Length > 0)
                    {
                        var sessions = await _context.Sessions
                            .Where(s => selectedSessions.Contains(s.Id) && s.ConferenceId == existingRegistration.ConferenceId)
                            .ToListAsync();

                        foreach (var session in sessions)
                        {
                            existingRegistration.Sessions.Add(session);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegistrationExists(registration.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = registration.Id });
            }

            ViewData["ConferenceId"] = new SelectList(_context.Conferences, "Id", "Name", registration.ConferenceId);
            ViewData["DelegateId"] = new SelectList(_context.Delegates.OrderBy(d => d.LastName).ThenBy(d => d.FirstName), "Id", "FullName", registration.DelegateId);
            ViewData["PaymentStatusList"] = new SelectList(Enum.GetValues(typeof(PaymentStatus)).Cast<PaymentStatus>(), registration.PaymentStatus);

            // Get available sessions for this conference
            ViewData["AvailableSessions"] = await _context.Sessions
                .Where(s => s.ConferenceId == registration.ConferenceId)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return View(registration);
        }

        // GET: Registrations/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registration = await _context.Registrations
                .Include(r => r.Conference)
                .Include(r => r.Delegate)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (registration == null)
            {
                return NotFound();
            }

            return View(registration);
        }

        // POST: Registrations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var registration = await _context.Registrations.FindAsync(id);
            if (registration == null)
            {
                return NotFound();
            }

            var conferenceId = registration.ConferenceId;
            var delegateId = registration.DelegateId;

            _context.Registrations.Remove(registration);
            await _context.SaveChangesAsync();

            // Determine where to redirect based on what view the user likely came from
            if (Request.Headers["Referer"].ToString().Contains($"Delegates/Details/{delegateId}"))
            {
                return RedirectToAction("Details", "Delegates", new { id = delegateId });
            }
            else if (Request.Headers["Referer"].ToString().Contains($"Conferences/Details/{conferenceId}"))
            {
                return RedirectToAction("Details", "Conferences", new { id = conferenceId });
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        private bool RegistrationExists(int id)
        {
            return _context.Registrations.Any(e => e.Id == id);
        }

        private string GenerateUniqueRegistrationCode()
        {
            string code;
            bool codeExists;

            do
            {
                // Generate a random 8-character alphanumeric code
                code = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 8)
                    .Select(s => s[new Random().Next(s.Length)]).ToArray());

                codeExists = _context.Registrations.Any(r => r.RegistrationCode == code);

            } while (codeExists);

            return code;
        }
    }
}