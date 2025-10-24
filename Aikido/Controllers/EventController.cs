using Aikido.Application.Services;
using Aikido.Dto;
using Aikido.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Controllers
{
    //[ApiController]
    //[Route("api/[controller]")]
    //public class EventController : ControllerBase
    //{
    //    private readonly EventApplicationService _eventApplicationService;

    //    public EventController(EventApplicationService eventApplicationService)
    //    {
    //        _eventApplicationService = eventApplicationService;
    //    }

    //    [HttpGet("get/{id}")]
    //    public async Task<IActionResult> GetEventById(long id)
    //    {
    //        try
    //        {
    //            var eventData = await _eventApplicationService.GetEventByIdAsync(id);
    //            return Ok(eventData);
    //        }
    //        catch (KeyNotFoundException ex)
    //        {
    //            return NotFound(new { ex.Message });
    //        }
    //        catch (Exception ex)
    //        {
    //            return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
    //        }
    //    }

    //    [HttpGet("get/all")]
    //    public async Task<IActionResult> GetAllEvents()
    //    {
    //        try
    //        {
    //            var events = await _eventApplicationService.GetAllEventsAsync();
    //            return Ok(events);
    //        }
    //        catch (Exception ex)
    //        {
    //            return StatusCode(500, new { Message = "Ошибка при получении списка событий", Details = ex.Message });
    //        }
    //    }

    //    [HttpGet("get/by-date-range")]
    //    public async Task<IActionResult> GetEventsByDateRange(
    //        [FromQuery] DateTime startDate,
    //        [FromQuery] DateTime endDate)
    //    {
    //        try
    //        {
    //            var events = await _eventApplicationService.GetEventsByDateRangeAsync(startDate, endDate);
    //            return Ok(events);
    //        }
    //        catch (Exception ex)
    //        {
    //            return StatusCode(500, new { Message = "Ошибка при получении событий по датам", Details = ex.Message });
    //        }
    //    }

    //    [HttpPost("create")]
    //    public async Task<IActionResult> CreateEvent([FromForm] EventRequest request)
    //    {
    //        EventDto eventData;
    //        try
    //        {
    //            eventData = await request.Parse();
    //        }
    //        catch (Exception ex)
    //        {
    //            return BadRequest($"Ошибка при обработке JSON: {ex.Message}");
    //        }

    //        try
    //        {
    //            var eventId = await _eventApplicationService.CreateEventAsync(eventData);
    //            return Ok(new { id = eventId });
    //        }
    //        catch (ArgumentException ex)
    //        {
    //            return BadRequest(ex.Message);
    //        }
    //        catch (Exception ex)
    //        {
    //            return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
    //        }
    //    }

    //    [HttpPut("update/{id}")]
    //    public async Task<IActionResult> UpdateEvent(long id, [FromForm] EventRequest request)
    //    {
    //        EventDto eventData;
    //        try
    //        {
    //            eventData = await request.Parse();
    //        }
    //        catch (Exception ex)
    //        {
    //            return BadRequest($"Ошибка при обработке JSON: {ex.Message}");
    //        }

    //        try
    //        {
    //            await _eventApplicationService.UpdateEventAsync(id, eventData);
    //            return Ok();
    //        }
    //        catch (KeyNotFoundException ex)
    //        {
    //            return NotFound(new { ex.Message });
    //        }
    //        catch (Exception ex)
    //        {
    //            return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
    //        }
    //    }

    //    [HttpDelete("delete/{id}")]
    //    public async Task<IActionResult> DeleteEvent(long id)
    //    {
    //        try
    //        {
    //            await _eventApplicationService.DeleteEventAsync(id);
    //            return Ok();
    //        }
    //        catch (KeyNotFoundException ex)
    //        {
    //            return NotFound(new { ex.Message });
    //        }
    //        catch (Exception ex)
    //        {
    //            return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
    //        }
    //    }
    //}
}
