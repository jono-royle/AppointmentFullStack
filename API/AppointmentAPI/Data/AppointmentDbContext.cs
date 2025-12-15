using AppointmentAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace AppointmentAPI.Data
{
    public class AppointmentDbContext : DbContext
    {
        public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options) : base(options)
        {
        }

        internal DbSet<Appointment> Appointments { get; set; } = null!;
    }
}
