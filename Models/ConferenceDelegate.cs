using System.ComponentModel.DataAnnotations;

namespace ConferenceDelegateManagement1234122.Models
{
    public class ConferenceDelegate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(12)]
        public string CCCD { get; set; } // Thay IdentityNumber bằng CCCD

        public string Email { get; set; }

        public List<Schedule>? Schedules { get; set; } = new List<Schedule>();
    }
}