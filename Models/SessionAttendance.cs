using ConferenceDelegateManagement1234122.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConferenceDelegateManagement1234122.Models
{
    /// <summary>
    /// Represents a delegate's attendance at a specific session
    /// </summary>
    public class SessionAttendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SessionId { get; set; }

        [Required]
        public int RegistrationId { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Check-in Time")]
        public DateTime? CheckInTime { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Check-out Time")]
        public DateTime? CheckOutTime { get; set; }

        [Display(Name = "Is Confirmed")]
        public bool IsConfirmed { get; set; } = false;

        [Display(Name = "Feedback Rating")]
        [Range(1, 5)]
        public int? FeedbackRating { get; set; }

        [Display(Name = "Feedback Comments")]
        [StringLength(1000)]
        public string? FeedbackComments { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("SessionId")]
        public virtual Session? Session { get; set; }

        [ForeignKey("RegistrationId")]
        public virtual Registration? Registration { get; set; }
    }
}