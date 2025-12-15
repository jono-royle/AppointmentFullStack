using AppointmentAPI.DTOs;
namespace AppointmentAPI.Services
{
    public interface IAppointmentIngestionService
    {
        Task<AppointmentDTO?> GetAppointmentFromId(Guid id);

        Task<AppointmentIngestionResult> IngestAppointment(AppointmentDTO appointment);
    }
}
