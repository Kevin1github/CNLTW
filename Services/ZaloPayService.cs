using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ConferenceDelegateManagement1234122.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConferenceDelegateManagement1234122.Services
{
    public interface IZaloPayService
    {
        Task<string> CreatePaymentUrl(int registrationId, decimal amount);
        bool ValidateCallback(ZaloPayCallback callback);
    }

    public class ZaloPayService : IZaloPayService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ZaloPayService> _logger;
        private readonly HttpClient _httpClient;
        private readonly ZaloPayConfig _config;

        public ZaloPayService(IConfiguration configuration, ILogger<ZaloPayService> logger, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
            
            _config = new ZaloPayConfig
            {
                AppId = _configuration["ZaloPay:AppId"],
                Key1 = _configuration["ZaloPay:Key1"],
                Key2 = _configuration["ZaloPay:Key2"],
                CreateOrderUrl = _configuration["ZaloPay:CreateOrderUrl"],
                CallbackUrl = _configuration["ZaloPay:CallbackUrl"]
            };
        }

        public async Task<string> CreatePaymentUrl(int registrationId, decimal amount)
        {
            try
            {
                var appTransId = DateTime.Now.ToString("yyMMdd") + "_" + registrationId;
                var embedData = JsonSerializer.Serialize(new { registrationId });

                var order = new ZaloPayCreateOrder
                {
                    AppId = _config.AppId,
                    AppUser = "user_" + registrationId,
                    AppTime = DateTimeOffset.Now.ToUnixTimeSeconds().ToString(),
                    Amount = ((long)amount).ToString(),
                    AppTransId = appTransId,
                    EmbedData = embedData,
                    Item = "[]",
                    Description = $"Thanh toan dang ky hoi nghi - Ma {registrationId}",
                    BankCode = "zalopayapp",
                };

                // Tạo chuỗi MAC
                var data = _config.AppId + "|" + order.AppTransId + "|" + order.AppUser + "|" + order.Amount + "|"
                    + order.AppTime + "|" + order.EmbedData + "|" + order.Item;
                order.Mac = HmacSHA256(data, _config.Key1);

                // Gọi API ZaloPay
                var content = new StringContent(JsonSerializer.Serialize(order), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_config.CreateOrderUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    if (result.GetProperty("returncode").GetInt32() == 1)
                    {
                        return result.GetProperty("orderurl").GetString();
                    }
                }

                _logger.LogError($"Failed to create ZaloPay order: {responseContent}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ZaloPay payment URL");
                return null;
            }
        }

        public bool ValidateCallback(ZaloPayCallback callback)
        {
            try
            {
                // Kiểm tra mã trả về
                if (callback.ReturnCode != 1)
                {
                    _logger.LogWarning($"ZaloPay callback failed with code {callback.ReturnCode}: {callback.ReturnMessage}");
                    return false;
                }

                // Tạo chuỗi data để verify
                var data = callback.AppId + "|" + callback.AppTransId + "|" + callback.AppUser + "|" + callback.Amount + "|"
                    + callback.AppTime + "|" + callback.EmbedData + "|" + callback.Item;
                var mac = HmacSHA256(data, _config.Key2);

                // Verify MAC
                if (mac != callback.Mac)
                {
                    _logger.LogWarning("Invalid MAC in ZaloPay callback");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating ZaloPay callback");
                return false;
            }
        }

        private string HmacSHA256(string data, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            using var hmac = new HMACSHA256(keyBytes);
            byte[] hashBytes = hmac.ComputeHash(dataBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
} 