namespace AppointmentAPI.Properties
{
    public class AppointmentIngestionOptions
    {
        /// <summary>
        /// The minimum time in the future the appointment must be created.
        /// </summary>
        public int FutureAppointmentTimeThresholdMinutes { get; set; }


        /// <summary>
        /// The default service duration in minutes when not specified.
        /// </summary>
        public int DefaultServiceDurationMinutes { get; set; }
    }
}
