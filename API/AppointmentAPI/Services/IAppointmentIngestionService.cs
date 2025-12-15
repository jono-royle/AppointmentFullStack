using AppointmentAPI.DTOs;
using AppointmentAPI.Models;

namespace AppointmentAPI.Services
{
    public interface IAppointmentIngestionService
    {
        Task<AppointmentDTO?> GetAppointmentFromId(Guid id);

        Task<AppointmentIngestionResult> IngestAppointment(AppointmentDTO appointment);
    }
}
