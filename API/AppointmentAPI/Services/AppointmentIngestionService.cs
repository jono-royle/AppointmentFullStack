using AppointmentAPI.DTOs;
using AppointmentAPI.Logging;
using AppointmentAPI.Models;
using AppointmentAPI.Properties;
using AppointmentAPI.Repositories;
using Microsoft.Extensions.Options;

namespace AppointmentAPI.Services
{
    internal class AppointmentIngestionService : IAppointmentIngestionService
    {
        private IAppointmentRepository _repository;
        private readonly AppointmentIngestionOptions _options;
        private IApiLogger _logger;

        public AppointmentIngestionService(IAppointmentRepository repository, IOptions<AppointmentIngestionOptions> options, IApiLogger logger)
        {
            _repository = repository;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<AppointmentDTO?> GetAppointmentFromId(Guid id)
        {
            var appointment = await _repository.GetByIdAsync(id);
            if (appointment == null)
            {
                _logger.Log($"Error - appointment {id} not found");
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
                appointment = ValidateAppointment(appointmentDTO, out errors, _options.FutureAppointmentTimeThresholdMinutes, _options.DefaultServiceDurationMinutes);
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
                _logger.Log($"Error - appointment not ingested due to {errors.Count} validation failures:");
                return ReportErrors(errors);
            }
            else
            {
                var exisitingAppointments = await _repository.GetAllAppointmentsAsync();
                if (!CheckForAppointmentOverlaps(appointment, exisitingAppointments, out errors))
                {
                    _logger.Log($"Error - appointment not ingested due to appointment overlap");
                    return ReportErrors(errors);
                }

                await _repository.SaveAsync(appointment);
                return new AppointmentIngestionResult
                {
                    SuccessfulIngestion = true,
                    AppointmentId = appointment.Id,
                };
            }
        }

        private AppointmentIngestionResult ReportErrors(List<string> errors)
        {
            foreach (var error in errors)
            {
                _logger.Log(error);
            }
            return new AppointmentIngestionResult
            {
                SuccessfulIngestion = false,
                ErrorMessages = errors
            };
        }

        private static Appointment? ValidateAppointment(AppointmentDTO appointment, out List<string> errors, int futureThreshold, int defaultServiceDuration)
        {
            errors = new List<string>();
            var utcAppointmentTime = appointment.AppointmentTime!.Value.UtcDateTime;
            if (utcAppointmentTime < DateTime.UtcNow + TimeSpan.FromMinutes(futureThreshold))
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
                appointment.ServiceDurationMinutes = defaultServiceDuration;
            }

            return new Appointment
            {
                Id = Guid.NewGuid(),
                AppointmentTime = FormatDateForDatabase(utcAppointmentTime),
                ServiceDurationMinutes = appointment.ServiceDurationMinutes.Value,
                ClientName = appointment.ClientName
            };
        }

        private static bool CheckForAppointmentOverlaps(Appointment appointment, List<Appointment> existingAppointments, out List<string> errors)
        {
            errors = new List<string>();
            var appointmentEnd = appointment.AppointmentTime + TimeSpan.FromMinutes(appointment.ServiceDurationMinutes);
            foreach (var existingAppointment in existingAppointments)
            {
                var existingEnd = existingAppointment.AppointmentTime + TimeSpan.FromMinutes(appointment.ServiceDurationMinutes);
                if(existingAppointment.AppointmentTime < appointmentEnd && appointment.AppointmentTime < existingEnd)
                {
                    errors.Add($"Overlapping appointment Id: {existingAppointment.Id}");
                    return false;
                }
            }
            return true;
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
