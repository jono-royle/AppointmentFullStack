using AppointmentAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace AppointmentAPI.DTOs
{
    public class AppointmentDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public required string ClientName { get; set; }
        [Required]
        public required DateTimeOffset AppointmentTime { get; set; }
        public int? ServiceDurationMinutes { get; set; }
    }
}
