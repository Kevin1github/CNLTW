using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConferenceDelegateManagement1234122.Models
{
    public class RestStop
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerNight { get; set; }

        [Range(0, int.MaxValue)]
        public int AvailableRooms { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int ConferenceId { get; set; }

        // Navigation property
        public virtual Conference? Conference { get; set; }
        public virtual ICollection<RestStopBooking> Bookings { get; set; } = new List<RestStopBooking>();

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 