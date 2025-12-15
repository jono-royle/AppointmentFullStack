using AppointmentAPI.Models;
using System.Collections.Concurrent;

namespace AppointmentAPI.Repositories
{
    internal class DictionaryAppointmentRepository : IAppointmentRepository
    {
        private ConcurrentDictionary<Guid, Appointment> _appointments;

        public DictionaryAppointmentRepository() 
        { 
            _appointments = new ConcurrentDictionary<Guid, Appointment>();
        }

        public Task<Appointment?> GetByIdAsync(Guid id)
        {
            _appointments.TryGetValue(id, out var appointment);
            return Task.FromResult(appointment);
        }

        public Task<Guid> SaveAsync(Appointment appointment)
        {
            _appointments.TryAdd(appointment.Id, appointment);
            return Task.FromResult(appointment.Id);
        }
    }
}
