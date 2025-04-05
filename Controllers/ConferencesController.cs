using ConferenceDelegateManagement1234122.Data;
using ConferenceDelegateManagement1234122.Models;
using ConferenceDelegateManagement1234122.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

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
        [AllowAnonymous]
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
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conference = await _context.Conferences
                .Include(c => c.Sessions)
                .Include(c => c.RestStops)
                .Include(c => c.Registrations)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (conference == null)
            {
                return NotFound();
            }

            // Load delegate details only for Admin
            if (User.IsInRole("Admin"))
            {
                conference = await _context.Conferences
                    .Include(c => c.Sessions)
                    .Include(c => c.Registrations)
                        .ThenInclude(r => r.Delegate)
                    .Include(c => c.RestStops)
                    .FirstOrDefaultAsync(m => m.Id == id);
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
        public async Task<IActionResult> Create([Bind("Name,Description,StartDate,EndDate,Location,MaximumDelegates,RegistrationDeadline,OrganizerEmail,OrganizerPhone,IsActive,RegistrationFee,AllowedPaymentMethods")] Conference conference)
        {
            if (ModelState.IsValid)
            {
                // Ensure at least one payment method is selected
                if (string.IsNullOrEmpty(conference.AllowedPaymentMethods))
                {
                    ModelState.AddModelError("AllowedPaymentMethods", "At least one payment method must be selected.");
                    return View(conference);
                }

                conference.CreatedAt = DateTime.UtcNow;
                conference.UpdatedAt = DateTime.UtcNow;
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

        // GET: Conferences/GenerateQRCode/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GenerateQRCode(int id)
        {
            var registration = await _context.Registrations
                .Include(r => r.Delegate)
                .Include(r => r.Conference)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registration == null)
            {
                return NotFound();
            }

            // Check if registration fee exists
            if (registration.Conference.RegistrationFee <= 0)
            {
                return BadRequest("Registration fee must be greater than zero");
            }

            try
            {
                // Build payment URL with proper URL encoding
                var paymentUrl = $"https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?vnp_Amount={registration.Conference.RegistrationFee * 100}&vnp_Command=pay&vnp_CreateDate={DateTime.Now:yyyyMMddHHmmss}&vnp_CurrCode=VND&vnp_IpAddr={HttpContext.Connection.RemoteIpAddress}&vnp_Locale=vn&vnp_OrderInfo={Uri.EscapeDataString("Conference Registration")}&vnp_OrderType=other&vnp_ReturnUrl={Uri.EscapeDataString(Url.Action("PaymentCallback", "Conferences", null, Request.Scheme))}&vnp_TmnCode=YOUR_TMN_CODE&vnp_TxnRef={registration.RegistrationCode}&vnp_Version=2.1&vnp_SecureHash=YOUR_HASH";

                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(paymentUrl, QRCodeGenerator.ECCLevel.Q);
                    using (var qrCode = new BitmapByteQRCode(qrCodeData))
                    {
                        var qrCodeImage = qrCode.GetGraphic(20);
                        return File(qrCodeImage, "image/png");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code");
                return StatusCode(500, "Error generating QR code");
            }
        }

        // POST: Conferences/MarkPaymentAsPaid/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkPaymentAsPaid(int id)
        {
            var registration = await _context.Registrations.FindAsync(id);
            if (registration == null)
            {
                return NotFound();
            }

            registration.PaymentStatus = PaymentStatus.Paid;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = registration.ConferenceId });
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