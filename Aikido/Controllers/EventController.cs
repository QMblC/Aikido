using Aikido.Dto;
using Aikido.Requests;
using Aikido.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : Controller
    {
        private readonly EventService eventService;

        public EventController(EventService eventService)
        {
            this.eventService = eventService;
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetEventById(long id)
        {
            try
            {
                var ev = await eventService.GetEventById(id);
                return Ok(ev);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Внутренняя ошибка сервера",
                    Details = ex.Message
                });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] EventRequest request)
        {
            EventDto eventData;

            try
            {
                eventData = await request.Parse();
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при обработке данных мероприятия: {ex.Message}");
            }

            long eventId;

            try
            {
                eventId = await eventService.CreateEvent(eventData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Внутренняя ошибка сервера",
                    Details = ex.Message
                });
            }

            return Ok(new { id = eventId });
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(long id, [FromForm] EventRequest request)
        {
            EventDto eventData;

            try
            {
                eventData = await request.Parse();
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при обработке данных мероприятия: {ex.Message}");
            }

            try
            {
                await eventService.UpdateEvent(id, eventData);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Внутренняя ошибка сервера",
                    Details = ex.Message
                });
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                await eventService.DeleteEvent(id);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Внутренняя ошибка сервера",
                    Details = ex.Message
                });
            }
        }
    }
}