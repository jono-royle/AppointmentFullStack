using AppointmentAPI.Data;
using AppointmentAPI.Models;
using System;
using Microsoft.EntityFrameworkCore;

namespace AppointmentAPI.Repositories
{
    internal class EfCoreAppointmentRepository : IAppointmentRepository
    {
        private readonly AppointmentDbContext _context;

        public EfCoreAppointmentRepository(AppointmentDbContext context)
        {
            _context = context;
        }

        public async Task<Appointment?> GetByIdAsync(Guid id)
        {
            return await _context.Appointments.FindAsync(id);
        }

        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            return await _context.Appointments.ToListAsync();
        }

        public async Task<Guid> SaveAsync(Appointment appointment)
        {
            // Add the entity
            await _context.Appointments.AddAsync(appointment);

            // Persist to the database
            await _context.SaveChangesAsync();

            return appointment.Id;
        }
    }
}
