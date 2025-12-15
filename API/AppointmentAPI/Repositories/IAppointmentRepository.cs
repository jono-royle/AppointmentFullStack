using AppointmentAPI.Models;

namespace AppointmentAPI.Repositories
{
    public interface IAppointmentRepository
    {
        Task<Appointment?> GetByIdAsync(Guid id);

        Task<Guid> SaveAsync(Appointment appointment);
    }
}
