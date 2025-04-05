using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ConferenceDelegateManagement1234122.Models.Enums;

namespace ConferenceDelegateManagement1234122.Models
{
    public class Conference
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        //[Required]
        [StringLength(200)]
        public string Venue { get; set; } = string.Empty;

        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        [Display(Name = "Maximum Delegates")]
        public int MaximumDelegates { get; set; }

        [Display(Name = "Registration Deadline")]
        [DataType(DataType.Date)]
        public DateTime? RegistrationDeadline { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Registration Fee")]
        [Range(0, double.MaxValue, ErrorMessage = "Registration fee must be greater than or equal to 0")]
        public decimal RegistrationFee { get; set; }

        [Required]
        [Display(Name = "Allowed Payment Methods")]
        public string AllowedPaymentMethods { get; set; } = "Cash,QRCode"; // Default to both methods

        [StringLength(1000)]
        public string? AdditionalDetails { get; set; }

        [EmailAddress]
        public string? OrganizerEmail { get; set; }

        [StringLength(20)]
        public string? OrganizerPhone { get; set; }

        public int MaxAttendees { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime Date { get; set; }

        // Navigation properties
        public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
        public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
        public virtual ICollection<RestStop> RestStops { get; set; } = new List<RestStop>();

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Tổng số đại biểu (delegates)
        public int TotalDelegates => Sessions?.Sum(s => s.Registrations?.Count ?? 0) ?? 0;
    }

    public static class ConferenceExtensions
    {
        // Tổng số hội thảo (conferences)
        public static int TotalConferences(this IEnumerable<Conference> conferences)
            => conferences?.Count() ?? 0;

        // Tổng số sessions
        public static int TotalSessions(this IEnumerable<Conference> conferences)
            => conferences?.Sum(c => c.Sessions?.Count ?? 0) ?? 0;

        // Danh sách hội thảo sắp diễn ra
        public static List<Conference> UpcomingConferences(this IEnumerable<Conference> conferences)
            => conferences?.Where(c => c.StartDate >= DateTime.Now).ToList() ?? new List<Conference>();
    }
}