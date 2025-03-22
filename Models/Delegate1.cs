using System.ComponentModel.DataAnnotations;

namespace ConferenceDelegateManagement1234122.Models
{
    public class Delegate1
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? Organization { get; set; }

        [StringLength(100)]
        [Display(Name = "Job Title")]
        public string? JobTitle { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(500)]
        [Display(Name = "Dietary Requirements")]
        public string? DietaryRequirements { get; set; }

        [StringLength(500)]
        [Display(Name = "Special Requirements")]
        public string? SpecialRequirements { get; set; }

        // Navigation properties
        public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";
    }
}