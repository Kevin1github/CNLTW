using System;
using System.Text.Json;
using System.Threading.Tasks;
using ConferenceDelegateManagement1234122.Data;
using ConferenceDelegateManagement1234122.Models;
using ConferenceDelegateManagement1234122.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConferenceDelegateManagement1234122.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZaloPayController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IZaloPayService _zaloPayService;
        private readonly ILogger<ZaloPayController> _logger;

        public ZaloPayController(
            ApplicationDbContext context,
            IZaloPayService zaloPayService,
            ILogger<ZaloPayController> logger)
        {
            _context = context;
            _zaloPayService = zaloPayService;
            _logger = logger;
        }

        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePayment(int registrationId)
        {
            try
            {
                var registration = await _context.Registrations
                    .Include(r => r.Conference)
                    .FirstOrDefaultAsync(r => r.Id == registrationId);

                if (registration == null)
                {
                    return NotFound("Registration not found");
                }

                if (registration.PaymentStatus == Models.Enums.PaymentStatus.Paid)
                {
                    return BadRequest("Registration is already paid");
                }

                var paymentUrl = await _zaloPayService.CreatePaymentUrl(registrationId, registration.Conference.RegistrationFee);
                if (string.IsNullOrEmpty(paymentUrl))
                {
                    return StatusCode(500, "Failed to create payment URL");
                }

                return Ok(new { paymentUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ZaloPay payment");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("callback")]
        public async Task<IActionResult> Callback([FromBody] ZaloPayCallback callback)
        {
            try
            {
                // Validate callback
                if (!_zaloPayService.ValidateCallback(callback))
                {
                    return BadRequest("Invalid callback");
                }

                // Parse registrationId from embedData
                var embedData = JsonSerializer.Deserialize<JsonElement>(callback.EmbedData);
                var registrationId = embedData.GetProperty("registrationId").GetInt32();

                // Update registration
                var registration = await _context.Registrations
                    .FirstOrDefaultAsync(r => r.Id == registrationId);

                if (registration == null)
                {
                    return NotFound("Registration not found");
                }

                // Update payment status
                registration.PaymentStatus = Models.Enums.PaymentStatus.Paid;
                registration.PaymentDate = DateTime.UtcNow;
                registration.TransactionCode = callback.ZpTransId;

                await _context.SaveChangesAsync();

                return Ok(new { returncode = 1, returnmessage = "success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ZaloPay callback");
                return StatusCode(500, "Internal server error");
            }
        }
    }
} 