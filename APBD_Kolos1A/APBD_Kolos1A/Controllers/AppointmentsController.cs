using APBD_Kolos1A.Exceptions;
using APBD_Kolos1A.Models.DTOs;
using APBD_Kolos1A.Services;
using APBD_Kolos1A.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_Kolos1A.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController(IAppointmentService _appointmentService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppointment(int id)
    {
        try
        {
            var visit = await _appointmentService.GetAppointment(id);
            return Ok(visit);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddAppointment([FromBody] AppointmentPostDto appointment)
    {
        if (!appointment.Services.Any())
        {
            return BadRequest("At least one Service is required");
        }
    
        try
        {
            await _appointmentService.AddAppointment(appointment);
        }
        catch (ConflictException e)
        {
            return Conflict(e.Message);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        return CreatedAtAction(nameof(GetAppointment), new { id = appointment.AppointmentId }, appointment);
    }
}