using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicAPI.Data;
using ClinicAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace ClinicAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly ClinicContext _context;

        public AppointmentsController(ClinicContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "USER")]
        public async Task<ActionResult<IEnumerable<object>>> GetAppointments()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return BadRequest("User ID claim not found in token.");
                }

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest($"Invalid User ID claim in token: {userIdClaim}");
                }

                var appointments = await _context.Appointments
                    .Where(a => a.UserId == userId)
                    .Select(a => new
                    {
                        a.Id,
                        a.Date,
                        a.Location,
                        a.Status,
                        a.AppointmentType,
                        User = new { a.UserId, a.User.Name, a.User.Email }
                    })
                    .ToListAsync();

                if (appointments == null || appointments.Count == 0)
                {
                    return NotFound("No appointments found for the user.");
                }

                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving appointments: {ex.Message}");
            }
        }




        [HttpGet("admin")]
        [Authorize(Policy = "ADMIN")]
        public async Task<ActionResult<IEnumerable<object>>> AdminGetAppointments()
        {
            try
            {
                var appointments = await _context.Appointments
                    .Select(a => new
                    {
                        a.Id,
                        a.Date,
                        a.Location,
                        a.Status,
                        a.AppointmentType,
                        User = new { a.UserId, a.User.Name, a.User.Email }
                    })
                    .ToListAsync();

                if (appointments == null || appointments.Count == 0)
                {
                    return NotFound("No appointments found.");
                }

                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving appointments: {ex.Message}");
            }
        }



        [HttpPost]
        [Authorize(Policy = "USER")]
        public async Task<ActionResult<Appointment>> PostAppointment([FromBody] Appointment appointment)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                
                var userExists = await _context.Users.AnyAsync(u => u.Id == appointment.UserId);
                if (!userExists)
                {
                    return BadRequest($"El usuario con Id {appointment.UserId} no existe.");
                }

                
                if (appointment.Date.Date == DateTime.Now.Date)
                {
                    return BadRequest("No se puede agregar una cita para el mismo día actual.");
                }

                
                var existingAppointment = await _context.Appointments
                    .AnyAsync(a => a.UserId == appointment.UserId && a.Date.Date == appointment.Date.Date);
                if (existingAppointment)
                {
                    return BadRequest("Ya existe una cita para el mismo día.");
                }

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAppointments), new { id = appointment.Id }, appointment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al crear la cita: {ex.Message}");
            }
        }



        [HttpPut("{id}/cancel")]
        [Authorize(Policy = "USER")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                {
                    return NotFound("La cita no existe.");
                }

                
                if (appointment.Date <= DateTime.Now.AddHours(24))
                {
                    return BadRequest("No se puede cancelar la cita con menos de 24 horas de antelación.");
                }

                appointment.Status = "CANCELADA";
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al cancelar la cita: {ex.Message}");
            }
        }



        [HttpDelete("{id}")]
        [Authorize(Policy = "ADMIN")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                {
                    return NotFound("La cita no existe.");
                }

                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al eliminar la cita: {ex.Message}");
            }
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}
