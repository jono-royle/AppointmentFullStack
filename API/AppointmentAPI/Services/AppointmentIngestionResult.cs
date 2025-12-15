namespace AppointmentAPI.Services
{
    public class AppointmentIngestionResult
    {
        public AppointmentIngestionResult()
        {
            ErrorMessages = new List<string>();
        }

        public Guid? AppointmentId { get; set; }
        public bool SuccessfulIngestion { get; set; }
        public List<string> ErrorMessages { get; set; }
    }
}
