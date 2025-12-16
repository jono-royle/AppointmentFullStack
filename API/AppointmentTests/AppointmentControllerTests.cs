using AppointmentAPI.Controllers;
using AppointmentAPI.DTOs;
using AppointmentAPI.Logging;
using AppointmentAPI.Models;
using AppointmentAPI.Properties;
using AppointmentAPI.Repositories;
using AppointmentAPI.Responses;
using AppointmentAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace AppointmentTests
{
    [TestClass]
    public sealed class AppointmentControllerTests
    {
        private AppointmentController _controller = null!;
        private Mock<IAppointmentRepository> _repository = null!;
        private static Guid _testAppointment1Id = Guid.NewGuid();

        private static int _defaultServiceDuration = 30;
        private static int _futureAppointmentThreshold = 5;

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
            var options = Options.Create(new AppointmentIngestionOptions
            {
                DefaultServiceDurationMinutes = _defaultServiceDuration,
                FutureAppointmentTimeThresholdMinutes = _futureAppointmentThreshold
            });
            var logger = new Mock<IApiLogger>();
            var ingestionService = new AppointmentIngestionService(_repository.Object, options, logger.Object);
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

        [TestMethod]
        public async Task InjestCorrectAppointment_ReturnsOkId()
        {
            var currentDate = DateTime.UtcNow;
            _repository.Setup(r => r.GetAllAppointmentsAsync()).ReturnsAsync(new List<Appointment>());
            var dto = new AppointmentDTO { 
                AppointmentTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, 0, 0,
                    DateTimeKind.Utc) + TimeSpan.FromHours(2),
                ClientName = "TestCreation"
            };
            var result = await _controller.Ingest(dto);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var success = okResult.Value as SuccessfulIngestionResponse;
            Assert.IsNotNull(success);
            Assert.IsNotNull(success.AppointmentId);
        }

        [TestMethod]
        public async Task IngestAppointment_StartsInPast_ReturnsError()
        {
            _repository.Setup(r => r.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment>());

            var currentDate = DateTime.UtcNow;
            var dto = new AppointmentDTO
            {
                AppointmentTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, 0, 0,
                    DateTimeKind.Utc) + TimeSpan.FromMinutes(-30),
                ClientName = "PastClient"
            };

            var result = await _controller.Ingest(dto);

            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            var error = badRequest.Value as ErrorResponse;
            Assert.IsNotNull(error);
            Assert.IsTrue(error.Errors.Count == 1);
            _repository.Verify(r => r.SaveAsync(It.IsAny<Appointment>()), Times.Never);
        }

        [TestMethod]
        public async Task IngestAppointment_StartsLessThanFutureThresholdInFuture_ReturnsError()
        {
            _repository.Setup(r => r.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment>());
            var currentDate = DateTime.UtcNow;
            var dto = new AppointmentDTO
            {
                AppointmentTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, 0, 0,
                    DateTimeKind.Utc) + TimeSpan.FromMinutes(_futureAppointmentThreshold - 1),
                ClientName = "SoonClient"
            };

            var result = await _controller.Ingest(dto);

            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            var error = badRequest.Value as ErrorResponse;
            Assert.IsNotNull(error);
            Assert.IsTrue(error.Errors.Count == 2);
        }

        [TestMethod]
        public async Task IngestAppointment_InvalidMinuteValue_ReturnsError()
        {
            _repository.Setup(r => r.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment>());

            var currentDate = DateTime.UtcNow;

            var dto = new AppointmentDTO
            {
                AppointmentTime = new DateTime(
                    currentDate.Year,
                    currentDate.Month,
                    currentDate.Day,
                    currentDate.Hour,
                    15,  
                    0,
                    DateTimeKind.Utc) + TimeSpan.FromHours(1),
                ClientName = "InvalidMinuteClient"
            };

            var result = await _controller.Ingest(dto);

            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            var error = badRequest.Value as ErrorResponse;
            Assert.IsNotNull(error);
            Assert.IsTrue(error.Errors.Count == 1);
        }

        [TestMethod]
        public async Task IngestAppointment_ValidMinuteValue_ReturnsOkId()
        {
            _repository.Setup(r => r.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment>());

            var currentDate = DateTime.UtcNow;

            var dto = new AppointmentDTO
            {
                AppointmentTime = new DateTime(
                    currentDate.Year,
                    currentDate.Month,
                    currentDate.Day,
                    currentDate.Hour,
                    30,
                    0,
                    DateTimeKind.Utc) + TimeSpan.FromHours(1),
                ClientName = "InvalidMinuteClient"
            };

            var result = await _controller.Ingest(dto);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var success = okResult.Value as SuccessfulIngestionResponse;
            Assert.IsNotNull(success);
            Assert.IsNotNull(success.AppointmentId);
        }

        [TestMethod]
        public async Task IngestAppointment_InvalidMinuteValueAndPast_ReturnsMultipleErrors()
        {
            _repository.Setup(r => r.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment>());

            var currentDate = DateTime.UtcNow;


            var dto = new AppointmentDTO
            {
                AppointmentTime = new DateTime(
                    currentDate.Year,
                    currentDate.Month,
                    currentDate.Day,
                    currentDate.Hour,
                    15,
                    0,
                    DateTimeKind.Utc) - TimeSpan.FromHours(1),
                ClientName = "InvalidMinuteClient"
            };

            var result = await _controller.Ingest(dto);

            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            var error = badRequest.Value as ErrorResponse;
            Assert.IsNotNull(error);
            Assert.IsTrue(error.Errors.Count == 2);
        }

        [TestMethod]
        public async Task InjestIncorrectCharInClientName_ReturnsError()
        {
            var currentDate = DateTime.UtcNow;
            _repository.Setup(r => r.GetAllAppointmentsAsync()).ReturnsAsync(new List<Appointment>());
            var dto = new AppointmentDTO
            {
                AppointmentTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, 0, 0,
                    DateTimeKind.Utc) + TimeSpan.FromHours(2),
                ClientName = "BadName" + Environment.NewLine
            };
            var result = await _controller.Ingest(dto);
            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            var error = badRequest.Value as ErrorResponse;
            Assert.IsNotNull(error);
            Assert.IsTrue(error.Errors.Count == 1);
        }

        [TestMethod]
        public async Task IngestAppointment_NoServiceDuration_SetsDefault()
        {
            Appointment? savedAppointment = null;

            _repository
                .Setup(r => r.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment>());

            _repository
                .Setup(r => r.SaveAsync(It.IsAny<Appointment>()))
                .Callback<Appointment>(a => savedAppointment = a)
                .ReturnsAsync(Guid.NewGuid());

            var currentDate = DateTime.UtcNow;

            var dto = new AppointmentDTO
            {
                AppointmentTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, 0, 0,
                    DateTimeKind.Utc) + TimeSpan.FromHours(2),
                ClientName = "DefaultDurationClient",
                ServiceDurationMinutes = null
            };

            var result = await _controller.Ingest(dto);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            Assert.IsNotNull(savedAppointment);

            Assert.AreEqual(_defaultServiceDuration, savedAppointment!.ServiceDurationMinutes);
        }

        [TestMethod]
        public async Task IngestAppointment_0Duration_SetsDefault()
        {
            Appointment? savedAppointment = null;

            _repository
                .Setup(r => r.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment>());

            _repository
                .Setup(r => r.SaveAsync(It.IsAny<Appointment>()))
                .Callback<Appointment>(a => savedAppointment = a)
                .ReturnsAsync(Guid.NewGuid());

            var currentDate = DateTime.UtcNow;

            var dto = new AppointmentDTO
            {
                AppointmentTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, 0, 0,
                    DateTimeKind.Utc) + TimeSpan.FromHours(2),
                ClientName = "DefaultDurationClient",
                ServiceDurationMinutes = 0
            };

            var result = await _controller.Ingest(dto);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            Assert.IsNotNull(savedAppointment);

            Assert.AreEqual(_defaultServiceDuration, savedAppointment!.ServiceDurationMinutes);
        }

        [TestMethod]
        public async Task IngestAppointment_NegativeDuration_SetsDefault()
        {
            Appointment? savedAppointment = null;

            _repository
                .Setup(r => r.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment>());

            _repository
                .Setup(r => r.SaveAsync(It.IsAny<Appointment>()))
                .Callback<Appointment>(a => savedAppointment = a)
                .ReturnsAsync(Guid.NewGuid());

            var currentDate = DateTime.UtcNow;

            var dto = new AppointmentDTO
            {
                AppointmentTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, 0, 0,
                    DateTimeKind.Utc) + TimeSpan.FromHours(2),
                ClientName = "DefaultDurationClient",
                ServiceDurationMinutes = -1
            };

            var result = await _controller.Ingest(dto);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            Assert.IsNotNull(savedAppointment);

            Assert.AreEqual(_defaultServiceDuration, savedAppointment!.ServiceDurationMinutes);
        }

        [TestMethod]
        public async Task IngestAppointment_LocalDateTime_SetsUTCDateTime()
        {
            Appointment? savedAppointment = null;

            _repository
                .Setup(r => r.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment>());

            _repository
                .Setup(r => r.SaveAsync(It.IsAny<Appointment>()))
                .Callback<Appointment>(a => savedAppointment = a)
                .ReturnsAsync(Guid.NewGuid());

            var currentDate = DateTime.Now;

            var dto = new AppointmentDTO
            {
                AppointmentTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, 0, 0,
                    DateTimeKind.Local) + TimeSpan.FromHours(2),
                ClientName = "DefaultDurationClient",
                ServiceDurationMinutes = null
            };

            var result = await _controller.Ingest(dto);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            Assert.IsNotNull(savedAppointment);

            Assert.AreEqual(DateTimeKind.Utc, savedAppointment!.AppointmentTime.Kind);
        }

        [TestMethod]
        public void AppointmentDTO_ClientNameIsRequired()
        {
            var dto = new AppointmentDTO
            {
                AppointmentTime = DateTime.UtcNow.AddHours(1),
                ClientName = null!
            };

            var results = ValidateModel(dto);

            Assert.IsTrue(results.Any(r => r.MemberNames.Contains(nameof(AppointmentDTO.ClientName))));
        }

        [TestMethod]
        public void AppointmentDTO_AppointmentTimeIsRequired()
        {
            var dto = new AppointmentDTO
            {
                ClientName = "Test Client",
                AppointmentTime = null
            };

            var results = ValidateModel(dto);

            Assert.IsTrue(results.Any(r => r.MemberNames.Contains(nameof(AppointmentDTO.AppointmentTime))));
        }

        [TestMethod]
        public void AppointmentDTO_ServiceDurationIsNotRequired()
        {
            var dto = new AppointmentDTO
            {
                ClientName = "Test Client",
                AppointmentTime = DateTime.UtcNow
            };

            var results = ValidateModel(dto);

            Assert.IsFalse(results.Any(r => r.MemberNames.Contains(nameof(AppointmentDTO.ServiceDurationMinutes))));
        }

        [TestMethod]
        public async Task IngestAppointment_OverlapsExistingAppointment_ReturnsError()
        {
            var currentDate = DateTime.UtcNow;
            var existingAppointment = new Appointment
            {
                Id = Guid.NewGuid(),
                AppointmentTime = new DateTime(
                    currentDate.Year,
                    currentDate.Month,
                    currentDate.Day,
                    currentDate.Hour,
                    0,
                    0,
                    DateTimeKind.Utc) + TimeSpan.FromHours(2),
                ServiceDurationMinutes = 60,
                ClientName = "ExistingClient"
            };

            _repository.Setup(r => r.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment> { existingAppointment });

            var overlappingDto = new AppointmentDTO
            {
                AppointmentTime = new DateTime(
                    existingAppointment.AppointmentTime.Year,
                    existingAppointment.AppointmentTime.Month,
                    existingAppointment.AppointmentTime.Day,
                    existingAppointment.AppointmentTime.Hour,
                    existingAppointment.AppointmentTime.Minute,
                    0,
                    DateTimeKind.Utc) + TimeSpan.FromMinutes(30),
                ClientName = "OverlappingClient",
                ServiceDurationMinutes = 45
            };

            var result = await _controller.Ingest(overlappingDto);
            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            var error = badRequest.Value as ErrorResponse;
            Assert.IsNotNull(error);
            Assert.IsTrue(error.Errors.Count == 1);
        }

        [TestMethod]
        public async Task IngestAppointment_EnclosesExistingAppointment_ReturnsError()
        {
            var currentDate = DateTime.UtcNow;
            var existingAppointment = new Appointment
            {
                Id = Guid.NewGuid(),
                AppointmentTime = new DateTime(
                    currentDate.Year,
                    currentDate.Month,
                    currentDate.Day,
                    currentDate.Hour,
                    0,
                    0,
                    DateTimeKind.Utc) + TimeSpan.FromHours(2),
                ServiceDurationMinutes = 30,
                ClientName = "ExistingClient"
            };

            _repository.Setup(r => r.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment> { existingAppointment });

            var overlappingDto = new AppointmentDTO
            {
                AppointmentTime = new DateTime(
                    existingAppointment.AppointmentTime.Year,
                    existingAppointment.AppointmentTime.Month,
                    existingAppointment.AppointmentTime.Day,
                    existingAppointment.AppointmentTime.Hour,
                    45,
                    0,
                    DateTimeKind.Utc) - TimeSpan.FromHours(1),
                ClientName = "OverlappingClient",
                ServiceDurationMinutes = 60
            };

            var result = await _controller.Ingest(overlappingDto);
            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            var error = badRequest.Value as ErrorResponse;
            Assert.IsNotNull(error);
            Assert.IsTrue(error.Errors.Count == 1);
        }

        [TestMethod]
        public async Task IngestAppointment_PartiallyOverlapsExistingAppointment_ReturnsError()
        {
            var currentDate = DateTime.UtcNow;
            var existingAppointment = new Appointment
            {
                Id = Guid.NewGuid(),
                AppointmentTime = new DateTime(
                    currentDate.Year,
                    currentDate.Month,
                    currentDate.Day,
                    currentDate.Hour,
                    0,
                    0,
                    DateTimeKind.Utc) + TimeSpan.FromHours(2),
                ServiceDurationMinutes = 60,
                ClientName = "ExistingClient"
            };

            _repository.Setup(r => r.GetAllAppointmentsAsync())
                .ReturnsAsync(new List<Appointment> { existingAppointment });

            var overlappingDto = new AppointmentDTO
            {
                AppointmentTime = new DateTime(
                    existingAppointment.AppointmentTime.Year,
                    existingAppointment.AppointmentTime.Month,
                    existingAppointment.AppointmentTime.Day,
                    existingAppointment.AppointmentTime.Hour,
                    30,
                    0,
                    DateTimeKind.Utc) - TimeSpan.FromHours(1),
                ClientName = "OverlappingClient",
                ServiceDurationMinutes = 45
            };

            var result = await _controller.Ingest(overlappingDto);
            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            var error = badRequest.Value as ErrorResponse;
            Assert.IsNotNull(error);
            Assert.IsTrue(error.Errors.Count == 1);
        }

        private static IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }
    }
}
