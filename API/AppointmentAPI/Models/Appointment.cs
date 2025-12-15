namespace AppointmentAPI.Models
{
    internal class Appointment
    {
        public Guid Id { get; set; }

        public required string ClientName { get; set; }

        public required DateTime AppointmentTime { get; set; }

        public required int ServiceDurationMinutes { get; set; }
    }
}
