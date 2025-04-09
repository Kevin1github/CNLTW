using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ConferenceDelegateManagement1234122.Models.Enums;

namespace ConferenceDelegateManagement1234122.Models
{
    public class Registration
    {
        [Key]
        public int Id { get; set; }

        public virtual ICollection<SessionAttendance> SessionAttendances { get; set; } = new List<SessionAttendance>();

        // Foreign keys
        [Required]
        public int DelegateId { get; set; }

        [Required]
        public int ConferenceId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Payment Status")]
        public PaymentStatus PaymentStatus { get; set; }

        [Display(Name = "Payment Date")]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "Attendance Confirmed")]
        public bool AttendanceConfirmed { get; set; } = false;

        [Display(Name = "Registration Code")]
        public string RegistrationCode { get; set; } = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

        [Display(Name = "Transaction Code")]
        public string? TransactionCode { get; set; }

        [StringLength(500)]
        [Display(Name = "Additional Notes")]
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Delegate1? Delegate { get; set; }
        public virtual Conference? Conference { get; set; }

        // Many-to-many relationship with sessions
        public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}