using System;
using System.ComponentModel.DataAnnotations;

namespace ConferenceDelegateManagement1234122.Models
{
    public class ZaloPayCallback
    {
        public int ReturnCode { get; set; }
        public string ReturnMessage { get; set; }
        public long SubReturnCode { get; set; }
        public string SubReturnMessage { get; set; }
        public string ZpTransId { get; set; }
        public string AppId { get; set; }
        public string AppTransId { get; set; }
        public string AppTime { get; set; }
        public string AppUser { get; set; }
        public string Amount { get; set; }
        public string EmbedData { get; set; }
        public string Item { get; set; }
        public string Mac { get; set; }
        public string ServerTime { get; set; }
        public string Channel { get; set; }
        public string MerchantUserId { get; set; }
        public string UserFeeAmount { get; set; }
        public string DiscountAmount { get; set; }
    }

    public class ZaloPayCreateOrder
    {
        public string AppId { get; set; }
        public string AppUser { get; set; }
        public string AppTime { get; set; }
        public string Amount { get; set; }
        public string AppTransId { get; set; }
        public string EmbedData { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public string BankCode { get; set; }
        public string Mac { get; set; }
    }

    public class ZaloPayConfig
    {
        public string AppId { get; set; }
        public string Key1 { get; set; }
        public string Key2 { get; set; }
        public string CreateOrderUrl { get; set; }
        public string CallbackUrl { get; set; }
    }
} 