using Aikido.Application.Services;
using Aikido.Dto.Seminars;
using Aikido.Requests;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeminarController : ControllerBase
    {
        private readonly SeminarApplicationService _seminarApplicationService;

        public SeminarController(SeminarApplicationService seminarApplicationService)
        {
            _seminarApplicationService = seminarApplicationService;
        }

        [HttpGet("get/{id}")]
        public async Task<ActionResult<SeminarDto>> GetSeminarById(long id)
        {
            try
            {
                var seminar = await _seminarApplicationService.GetSeminarByIdAsync(id);
                return Ok(seminar);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        [HttpGet("get/all")]
        public async Task<ActionResult<List<SeminarDto>>> GetAllSeminars()
        {
            try
            {
                var seminars = await _seminarApplicationService.GetAllSeminarsAsync();
                return Ok(seminars);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении списка семинаров", Details = ex.Message });
            }
        }

        [HttpGet("get/{seminarId}/members")]
        public async Task<ActionResult<List<SeminarMemberDto>>> GetSeminarMembers(long seminarId)
        {
            try
            {
                var members = await _seminarApplicationService.GetSeminarMembersAsync(seminarId);
                return Ok(members);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении участников семинара", Details = ex.Message });
            }
        }

        [HttpDelete("{seminarId}/members/{userId}")]
        public async Task<IActionResult> RemoveMemberFromSeminar(long seminarId, long userId)
        {
            try
            {
                await _seminarApplicationService.RemoveMemberFromSeminarAsync(seminarId, userId);
                return Ok(new { Message = "Участник успешно удален из семинара" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateWithFiles([FromForm] SeminarRequest files)
        {
            if (files.SeminarData == null)
                return BadRequest("SeminarData обязательно");

            var seminar = files.SeminarData;
            seminar.ContactInfo = files.ContactInfo;
            seminar.Schedule = files.Schedule;
            seminar.Groups = files.Groups;

            var seminarId = await _seminarApplicationService.CreateSeminarAsync(seminar);

            if (files.PdfFile != null && files.PdfFile.Length > 0)
            {
                using var pdfMs = new MemoryStream();
                await files.PdfFile.CopyToAsync(pdfMs);
                var pdfBytes = pdfMs.ToArray();

                await _seminarApplicationService.AddSeminarRegulationAsync(seminarId, pdfBytes);
            }

            return Ok(new { id = seminarId });
        }



        [HttpGet("get/{seminarId}/regulation")]
        public async Task<IActionResult> DownloadSeminarRegulation(long seminarId)
        {
            var fileBytes = await _seminarApplicationService.GetSeminarRegulationAsync(seminarId);
            if (fileBytes == null || fileBytes.Length == 0)
            {
                return NotFound();
            }

            var contentType = "application/pdf";
            var fileName = $"SeminarRegulation_{seminarId}.pdf";

            return File(fileBytes, contentType, fileName);
        }

        [HttpDelete("delete/{seminarId}/regulation")]
        public async Task<IActionResult> DeleteSeminarRegulation(long seminarId)
        {
            await _seminarApplicationService.DeleteSeminarRegulationAsync(seminarId);
            return NoContent();
        }


        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateSeminar(long id, [FromBody] SeminarDto seminarData)
        {
            try
            {
                await _seminarApplicationService.UpdateSeminarAsync(id, seminarData);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteSeminar(long id)
        {
            try
            {
                await _seminarApplicationService.DeleteSeminarAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }
    }
}
