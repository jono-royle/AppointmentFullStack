using AppointmentAPI.DTOs;
using AppointmentAPI.Responses;
using AppointmentAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentAPI.Controllers
{
    /// <summary>
    /// Controller responsible for managing appointment-related operations,
    /// including ingesting new appointments and retrieving existing appointments.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private IAppointmentIngestionService _ingestionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppointmentController"/> class.
        /// </summary>
        /// <param name="ingestionService">
        /// The service responsible for handling appointment ingestion logic.
        /// </param>
        public AppointmentController(IAppointmentIngestionService ingestionService) 
        { 
            _ingestionService = ingestionService;
        }


        /// <summary>
        /// Ingests a new appointment.
        /// </summary>
        /// <param name="appointment">Appointment details including client name and appointment time.</param>
        /// <response code="200">Appointment ingested successfully.</response>
        /// <response code="400">Validation errors occurred.</response>
        [HttpPost("ingest")]
        [ProducesResponseType(typeof(SuccessfulIngestionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Ingest([FromBody] AppointmentDTO appointment)
        {
            var result = await _ingestionService.IngestAppointment(appointment);
            if(!result.SuccessfulIngestion)
            {
                return BadRequest(new ErrorResponse
                {
                    Errors = result.ErrorMessages
                });
            }
            else
            {
                return Ok(new SuccessfulIngestionResponse
                {
                    AppointmentId = result.AppointmentId!.Value
                });
            }
        }

        /// <summary>
        /// Retrieves an appointment by its unique identifier.
        /// </summary>
        /// <param name="id">The appointment ID.</param>
        /// <response code="200">The appointment was found.</response>
        /// <response code="404">The appointment does not exist.</response>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AppointmentDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetAppointment(Guid id) 
        {
            var appointment = await _ingestionService.GetAppointmentFromId(id);
            if (appointment == null) 
            { 
                return NotFound("Appointment not found");
            }
            return Ok(appointment);
        }

    }
}
