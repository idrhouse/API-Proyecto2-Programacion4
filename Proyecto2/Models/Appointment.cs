using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace ClinicAPI.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public string Location { get; set; }

        public string Status { get; set; } = "ACTIVA";

        public string AppointmentType { get; set; }

        public int UserId { get; set; }

        [JsonIgnore]
        public User User { get; set; }
    }
}
