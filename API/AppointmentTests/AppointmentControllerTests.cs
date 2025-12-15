using AppointmentAPI.Controllers;
using AppointmentAPI.DTOs;
using AppointmentAPI.Models;
using AppointmentAPI.Repositories;
using AppointmentAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AppointmentTests
{
    [TestClass]
    public sealed class AppointmentControllerTests
    {
        private AppointmentController _controller;
        private Mock<IAppointmentRepository> _repository;
        private static Guid _testAppointment1Id = Guid.NewGuid();

        private Appointment _testAppointment1 = new Appointment {
            Id = _testAppointment1Id,
            AppointmentTime = DateTime.UtcNow.AddMinutes(60),
            ClientName = "TestClient1",
            ServiceDurationMinutes = 60,
        };

        [TestInitialize]
        public void Setup()
        {
            _repository = new Mock<IAppointmentRepository>();
            _repository.Setup(r => r.GetByIdAsync(_testAppointment1Id)).ReturnsAsync(_testAppointment1);
            var ingestionService = new AppointmentIngestionService(_repository.Object);
            _controller = new AppointmentController(ingestionService);
        }

        [TestMethod]
        public async Task GetAppointment_ReturnsOkAppointment()
        {
            var result = await _controller.GetAppointment(_testAppointment1Id);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var appointment = okResult.Value as AppointmentDTO;
            Assert.IsNotNull(appointment);

            Assert.AreEqual(_testAppointment1.ClientName, appointment.ClientName);
        }

        [TestMethod]
        public async Task GetIncorrectAppointment_ReturnsNotFound()
        {
            var result = await _controller.GetAppointment(Guid.NewGuid());
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
        }
    }
}
