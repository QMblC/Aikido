using Aikido.Application.Services;
using Aikido.Dto.Seminars;
using Aikido.Dto.Seminars.Creation;
using Aikido.Dto.Seminars.Members;
using Aikido.Dto.Users;
using Aikido.Requests;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
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

        [HttpGet("{seminarId}/registered-coaches")]
        public async Task<ActionResult<List<UserShortDto>>> GetRegisteredCoaches(long seminarId)
        {
            try
            {
                var coaches = await _seminarApplicationService.GetRegisteredCoaches(seminarId);
                return Ok(coaches);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        [HttpGet("{seminarId}/registered-coach/{coachId}/members")]
        public async Task<ActionResult<List<SeminarMemberDto>>> GetCoachMembers(long seminarId, long coachId)
        {
            try
            {
                var members = await _seminarApplicationService.GetRegisteredCoachMembers(seminarId, coachId);
                return Ok(members);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        [HttpGet("{seminarId}/get/groups/members")]
        public async Task<ActionResult<List<SeminarMemberDto>>> GetSelectedGroupsMembers(long seminarId, [FromQuery] List<long> groupIds)
        {
            try
            {
                var members = await _seminarApplicationService.GetStartMemberInfoByGroups(seminarId, groupIds);

                return Ok(members);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        [HttpGet("{seminarId}/get-member/{userId}")]
        public async Task<ActionResult<SeminarMemberDto>> GetStartMembersData(long seminarId, long userId, [FromQuery] long coachId)
        {
            try
            {
                var member = await _seminarApplicationService.GetStartMemberdata(seminarId, userId, coachId);
                return Ok(member);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSeminar([FromBody] SeminarDto seminarData)
        {
            try
            {
                var seminarId = await _seminarApplicationService.CreateSeminarAsync(seminarData);
                return Ok(new { id = seminarId });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
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

        [HttpGet("{seminarId}/regulation")]
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

        [HttpPost("{seminarId}/regulation")]
        public async Task<IActionResult> SetSeminarRegulation(long seminarId, [FromForm] RegulationRequest request)
        {
            try
            {
                using var pdfMs = new MemoryStream();
                await request.RegulationFile.CopyToAsync(pdfMs);
                var filesInBytes = pdfMs.ToArray();

                await _seminarApplicationService.AddSeminarRegulationAsync(seminarId, filesInBytes);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        [HttpDelete("{seminarId}/regulation")]
        public async Task<IActionResult> DeleteSeminarRegulation(long seminarId)
        {
            try
            {
                await _seminarApplicationService.DeleteSeminarRegulationAsync(seminarId);
                return NoContent();
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
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

        [HttpPost("{seminarId}/members")]
        public async Task<IActionResult> CreateSeminarMembers(long seminarId, SeminarMemberGroupDto memberGroup)
        {
            try
            {
                await _seminarApplicationService.AddSeminarMembersAsync(seminarId, memberGroup);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex?.ToString() ?? "Неизвестная ошибка" });
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

        [HttpPut("{seminarId}/apply")]
        public async Task<IActionResult> ApplySeminarResult(long seminarId)
        {
            try
            {
                await _seminarApplicationService.ApplySeminarResult(seminarId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        [HttpPut("{seminarId}/cancel")]
        public async Task<IActionResult> CancelSeminarResult(long seminarId)
        {
            try
            {
                await _seminarApplicationService.CancelSeminarResult(seminarId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }
    }
}
