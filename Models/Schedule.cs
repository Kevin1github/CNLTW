using System.ComponentModel.DataAnnotations;

namespace ConferenceDelegateManagement1234122.Models
{
    public class Schedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public List<ConferenceDelegate>? Delegates { get; set; }

        public int ConferenceDelegateId { get; set; }
        public ConferenceDelegate ConferenceDelegate { get; set; }
    }
}