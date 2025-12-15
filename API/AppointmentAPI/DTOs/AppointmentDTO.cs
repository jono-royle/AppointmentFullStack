using AppointmentAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace AppointmentAPI.DTOs
{

    /// <summary>
    /// Data transfer object representing an appointment.
    /// Used when creating or returning appointment data via the API.
    /// </summary>
    public class AppointmentDTO
    {
        /// <summary>
        /// The name of the client for the appointment.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public required string ClientName { get; set; }

        /// <summary>
        /// The scheduled appointment time with timezone information.
        /// </summary>
        [Required]
        public required DateTimeOffset? AppointmentTime { get; set; }

        /// <summary>
        /// Duration of the service in minutes (optional).
        /// </summary>
        public int? ServiceDurationMinutes { get; set; }
    }
}
