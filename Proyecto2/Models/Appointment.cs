namespace ClinicAPI.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public string Status { get; set; } = "ACTIVA";
        public int PatientId { get; set; }
        public Patient Patient { get; set; }
        public string AppointmentType { get; set; }
    }
}
