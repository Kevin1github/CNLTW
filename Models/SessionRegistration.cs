using ConferenceDelegateManagement1234122.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConferenceDelegateManagement1234122.Models
{
    public class SessionRegistration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public bool HasAttended { get; set; } = false;

        [StringLength(500)]
        public string? Feedback { get; set; }

        // Foreign keys
        public int SessionId { get; set; }

        public int RegistrationId { get; set; }

        // Navigation properties
        [ForeignKey("SessionId")]
        public virtual Session Session { get; set; } = null!;

        [ForeignKey("RegistrationId")]
        public virtual Registration Registration { get; set; } = null!;
    }
}