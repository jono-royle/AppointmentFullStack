using AppointmentAPI.DTOs;
using AppointmentAPI.Responses;
using AppointmentAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private IAppointmentIngestionService _ingestionService;

        public AppointmentController(IAppointmentIngestionService ingestionService) 
        { 
            _ingestionService = ingestionService;
        }

        [HttpPost("ingest")]
        public async Task<ActionResult> Ingest(AppointmentDTO appointment)
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
                    Id = result.AppointmentId!.Value
                });
            }
        }

        [HttpGet("{id}")]
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
