using AppointmentAPI.Models;

namespace AppointmentAPI.Repositories
{
    internal interface IAppointmentRepository
    {
        Task<Appointment?> GetByIdAsync(Guid id);

        Task<Guid> SaveAsync(Appointment appointment);
    }
}
