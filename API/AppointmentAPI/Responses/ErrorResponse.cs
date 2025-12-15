namespace AppointmentAPI.Responses
{
    /// <summary>
    /// Represents an error response returned by the API when a request fails validation
    /// or encounters a processing error.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// A collection of error messages describing why the request was unsuccessful.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
    }
}
