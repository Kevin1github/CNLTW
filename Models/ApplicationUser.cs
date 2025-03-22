using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace ConferenceDelegateManagement1234122.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        //[Required]
        //[StringLength(12, MinimumLength = 9, ErrorMessage = "CCCD phải có từ 9 đến 12 chữ số.")]
        public string? CCCD { get; set; } = string.Empty; // Căn cước công dân

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Delegate"; // Role: Admin, Delegate

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}