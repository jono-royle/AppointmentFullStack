using AppointmentAPI.DTOs;
using AppointmentAPI.Models;
using AppointmentAPI.Repositories;

namespace AppointmentAPI.Services
{
    internal class AppointmentIngestionService : IAppointmentIngestionService
    {
        private IAppointmentRepository _repository;

        public AppointmentIngestionService(IAppointmentRepository repository)
        {
            _repository = repository;
        }

        public async Task<AppointmentDTO?> GetAppointmentFromId(Guid id)
        {
            var appointment = await _repository.GetByIdAsync(id);
            if (appointment == null)
            {
                Console.WriteLine($"Error - appointment {id} not found");
                return null;
            }

            return new AppointmentDTO
            {
                ClientName = appointment.ClientName,
                AppointmentTime = appointment.AppointmentTime,
                ServiceDurationMinutes = appointment.ServiceDurationMinutes,
            };
        }

        public async Task<AppointmentIngestionResult> IngestAppointment(AppointmentDTO appointmentDTO)
        {
            List<string> errors;
            Appointment? appointment = null;
            var clientName = appointmentDTO.ClientName;
            if (ValidateClientName(ref clientName))
            {
                appointmentDTO.ClientName = clientName;
                appointment = ValidateAppointment(appointmentDTO, out errors);
            }
            else
            {
                errors = new List<string>
                {
                    "Client name contains invalid characters"
                };
            }

            if (appointment == null)
            {
                Console.WriteLine($"Error - appointment not ingested due to {errors.Count} validation failures:");
                foreach (var error in errors) { 
                    Console.WriteLine(error);
                }
                return new AppointmentIngestionResult
                {
                    SuccessfulIngestion = false,
                    ErrorMessages = errors
                };
            }
            else
            {
                await _repository.SaveAsync(appointment);
                return new AppointmentIngestionResult
                {
                    SuccessfulIngestion = true,
                    AppointmentId = appointment.Id,
                };
            }
        }

        private static Appointment? ValidateAppointment(AppointmentDTO appointment, out List<string> errors)
        {
            errors = new List<string>();
            var utcAppointmentTime = appointment.AppointmentTime.UtcDateTime;
            if (utcAppointmentTime < DateTime.UtcNow + TimeSpan.FromMinutes(5))
            {
                errors.Add("Appointment time must be in the future.");
            }
            if(utcAppointmentTime.Minute != 0 && utcAppointmentTime.Minute != 30)
            {
                errors.Add("Appointment must start on the hour or half-hour.");
            }

            if (errors.Count > 0)
            {
                return null;
            }

            if(appointment.ServiceDurationMinutes == null || appointment.ServiceDurationMinutes == 0)
            {
                appointment.ServiceDurationMinutes = 30;
            }

            return new Appointment
            {
                Id = Guid.NewGuid(),
                AppointmentTime = FormatDateForDatabase(utcAppointmentTime),
                ServiceDurationMinutes = appointment.ServiceDurationMinutes.Value,
                ClientName = appointment.ClientName
            };
        }

        //Remove trailing whitespace and validate for control characters
        private static bool ValidateClientName(ref string input)
        {
            input.Trim();
            if (input.Any(char.IsControl))
            {
                return false;
            }
            return true;

        }

        //Ensure appointment times saved in the DB don't include seconds and are in UTC format
        private static DateTime FormatDateForDatabase(DateTime input) 
        {
            return new DateTime(input.Year, input.Month, input.Day, input.Hour, input.Minute, 0, DateTimeKind.Utc);
        }

    }
}
