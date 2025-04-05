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

        [StringLength(200)]
        public string? Address { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Company { get; set; } = string.Empty;

        [StringLength(100)]
        public string? JobTitle { get; set; } = string.Empty;

        public string? CCCD { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}