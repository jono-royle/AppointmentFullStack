namespace AppointmentAPI.Responses
{
    /// <summary>
    /// Represents a successful response returned by the API when an appointment
    /// is successfully ingested.
    /// </summary>
    public class SuccessfulIngestionResponse
    {
        /// <summary>
        /// The unique identifier of the newly created appointment.
        /// </summary>
        public Guid AppointmentId { get; set; }
        /// <summary>
        /// A human-readable message confirming that the appointment was created successfully
        /// </summary>
        public string Message { get; set; } = "Appointment created successfully.";
    }
}
