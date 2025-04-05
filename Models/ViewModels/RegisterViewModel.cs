using System;
using System.ComponentModel.DataAnnotations;
using ConferenceDelegateManagement1234122.Models.Enums;

namespace ConferenceDelegateManagement1234122.Models.ViewModels
{
    public class RegisterViewModel
    {
        public int ConferenceId { get; set; }
        
        public string ConferenceName { get; set; } = string.Empty;
        
        [Display(Name = "Registration Fee")]
        [DataType(DataType.Currency)]
        public decimal RegistrationFee { get; set; }
        
        [Required(ErrorMessage = "Please select a payment method")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "Cash";
    }
} 