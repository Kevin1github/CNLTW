using System.ComponentModel.DataAnnotations;
using ConferenceDelegateManagement1234122.Models;


namespace ConferenceDelegateManagement1234122.Models
{
    public class Session
    {
        public int Id { get; set; }

        public List<SessionAttendance> Attendances { get; set; } = new();

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }

        [StringLength(100)]
        public string? Speaker { get; set; }

        [StringLength(100)]
        public string? Room { get; set; }

        [Range(0, int.MaxValue)]
        [Display(Name = "Maximum Attendees")]
        public int? MaximumAttendees { get; set; }

        [Display(Name = "Track/Category")]
        public string? Track { get; set; }

        // Foreign key
        [Required]
        public int ConferenceId { get; set; }

        // Navigation property
        public virtual Conference? Conference { get; set; }

        // Registration sessions many-to-many
        public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}