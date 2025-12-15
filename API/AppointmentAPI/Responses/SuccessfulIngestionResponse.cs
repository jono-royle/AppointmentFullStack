namespace AppointmentAPI.Responses
{
    public class SuccessfulIngestionResponse
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = "Appointment created successfully.";
    }
}
