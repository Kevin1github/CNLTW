using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ConferenceDelegateManagement1234122.Models.Enums;

namespace ConferenceDelegateManagement1234122.Models
{
    public class RestStopBooking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RestStopId { get; set; }

        [Required]
        public int RegistrationId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Check-in Date")]
        public DateTime CheckInDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Check-out Date")]
        public DateTime CheckOutDate { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        [Display(Name = "Number of Rooms")]
        public int NumberOfRooms { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Price")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Payment Status")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [StringLength(500)]
        [Display(Name = "Special Requests")]
        public string? SpecialRequests { get; set; }

        // Navigation properties
        public virtual RestStop? RestStop { get; set; }
        public virtual Registration? Registration { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 