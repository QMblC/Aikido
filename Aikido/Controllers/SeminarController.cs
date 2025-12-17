using Aikido.AdditionalData.Enums;
using Aikido.Application.Services;
using Aikido.Dto;
using Aikido.Dto.Seminars;
using Aikido.Dto.Seminars.Creation;
using Aikido.Dto.Seminars.Members;
using Aikido.Dto.Seminars.Members.Creation;
using Aikido.Dto.Users;
using Aikido.Requests;
using Aikido.Services;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeminarController : ControllerBase
    {
        private readonly SeminarApplicationService _seminarApplicationService;
        private readonly TableService _tableService;

        public SeminarController(SeminarApplicationService seminarApplicationService, TableService tableService)
        {
            _seminarApplicationService = seminarApplicationService;
            _tableService = tableService;
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
            catch (Exception ex)
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
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        [HttpPost("{seminarId}/members")]
        public async Task<IActionResult> CreateSeminarMembers(long seminarId, SeminarMemberListDto memberGroup)
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

        [HttpPost("{seminarId}/final/members")]
        public async Task<IActionResult> CreateFinalSeminarMembers(long seminarId, List<FinalSeminarMemberDto> members)
        {
            try
            {
                await _seminarApplicationService.SetFinalSeminarMember(seminarId, members);
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

        /// <summary>
        /// Применяет результаты семинара
        /// </summary>
        /// <param name="seminarId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Отменяет результаты семинара
        /// </summary>
        /// <param name="seminarId"></param>
        /// <returns></returns>
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

        [HttpGet("{seminarId}/coach/{coachId}/members/table")]
        public async Task<IActionResult> GetCoachMembersTable(long seminarId, long coachId)
        {
            try
            {
                var members = await _seminarApplicationService.GetRegisteredCoachMembers(seminarId, coachId);

                if (members.Count == 0)
                {
                    return StatusCode(404, new { Message = "Не удалось найти участников" });
                }

                var stream = _tableService.CreateSeminarMembersTable(members);
                return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Members.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        [HttpGet("{seminarId}/final-statement/table")]
        public async Task<IActionResult> GetFinalStatementTable(long seminarId)
        {
            try
            {
                var members = await _seminarApplicationService.GetSeminarMembersAsync(seminarId);

                if (members.Count == 0)
                {
                    return StatusCode(404, new { Message = "Не удалось найти участников" });
                }

                var stream = _tableService.CreateSeminarMembersTable(members);
                return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Members.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        [HttpPost("{seminarId}/coach/{coachId}/members/table")]
        public async Task<IActionResult> CreateCoachMembersByTable(long seminarId, long coachId, [FromForm] TableFileRequest tableFile)
        {
            try
            {
                var file = tableFile.Table;

                if (file == null || file.Length == 0)
                    return BadRequest("Файл Excel не найден или пустой!");

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    var partialMembers = _tableService.ParseSeminarMembersTable(stream);

                    var fullCreationDataTasks = partialMembers.Select(async m =>
                    {
                        var creationMember = new SeminarMemberCreationDto()
                        {
                            UserId = m.UserId,
                            CertificationGrade = m.CertificationGrade,
                            SeminarPriceInRubles = m.SeminarPriceInRubles,
                            BudoPassportPriceInRubles = m.BudoPassportPriceInRubles,
                            AnnualFeePriceInRubles = m.AnnualFeePriceInRubles,   
                            CertificationPriceInRubles = m.CertificationPriceInRubles

                        };
                        return creationMember;
                    });

                    var members = await Task.WhenAll(fullCreationDataTasks);
                    var memberGroup = new SeminarMemberListDto() { CreatorId = coachId, Members = members.ToList() };

                    await _seminarApplicationService.AddSeminarMembersAsync(seminarId, memberGroup);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        [HttpPost("{seminarId}/final-statement/table")]
        public async Task<IActionResult> SetFinalStatement(long seminarId, [FromForm] TableFileRequest tableFile)
        {
            try
            {
                var file = tableFile.Table;

                if (file == null || file.Length == 0)
                    return BadRequest("Файл Excel не найден или пустой!");

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    var partialMembers = _tableService.ParseSeminarMembersTable(stream);

                    var fullCreationDataTasks = partialMembers.Select(async m =>
                    {
                        var creationMember = new FinalSeminarMemberDto()
                        {
                            UserId = m.UserId,
                            Status = m.CertificationGrade != null 
                            ? EnumParser.ConvertEnumToString(SeminarMemberStatus.Certified) 
                            : EnumParser.ConvertEnumToString(SeminarMemberStatus.Training),
                            CertificationGrade = m.CertificationGrade,
                            SeminarPriceInRubles = m.SeminarPriceInRubles,
                            BudoPassportPriceInRubles = m.BudoPassportPriceInRubles,
                            AnnualFeePriceInRubles = m.AnnualFeePriceInRubles,
                            CertificationPriceInRubles = m.CertificationPriceInRubles

                        };
                        return creationMember;
                    });

                    var members = await Task.WhenAll(fullCreationDataTasks);
                    await _seminarApplicationService.SetFinalSeminarMember(seminarId, members.ToList());
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        [HttpDelete("{seminarId}/coach/{coachId}/members")]
        public async Task<IActionResult> DeleteCoachMembers(long seminarId, long coachId)
        {
            try
            {
                var memberGroup = new SeminarMemberListDto() { CreatorId = coachId, Members = new() };

                await _seminarApplicationService.AddSeminarMembersAsync(seminarId, memberGroup);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        #region ManagerRequests

        /// <summary>
        /// Получение списка участников семинара конкретного руководителя
        /// </summary>
        /// <param name="seminarId"></param>
        /// <param name="managerId"></param>
        /// <returns></returns>
        [HttpGet("{seminarId}/requested-members/{managerId}")]
        public async Task<ActionResult<List<SeminarMemberManagerRequestDto>>> GetSeminarMembersManagerRequest(long seminarId,
            long managerId)
        {
            try
            {
                var members = await _seminarApplicationService.GetRequestedMembers(seminarId, managerId);
                return Ok(members);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Получение списка участников семинара конкретного клуба
        /// </summary>
        /// <param name="seminarId"></param>
        /// <param name="managerId"></param>
        /// <param name="clubId"></param>
        /// <returns></returns>
        [HttpGet("{seminarId}/requested-members/{managerId}/{clubId}")]
        public async Task<ActionResult<List<SeminarMemberManagerRequestDto>>> GetClubSeminarMembersManagerRequest(long seminarId,
            long managerId,
            long clubId)
        {
            try
            {
                var members = await _seminarApplicationService.GetClubRequestedMembers(seminarId, managerId, clubId);
                return Ok(members);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Поиск участника с указанным основным клубом по имени
        /// </summary>
        /// <param name="clubId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet("find/{clubId}/member")]
        public async Task<ActionResult<List<UserShortDto>>> FindClubMemberByName(long clubId, [FromQuery] string name)
        {
            try
            {
                var clubUsers = await _seminarApplicationService.FindClubMemberByName(clubId, name);
                return Ok(clubUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Получение стартовой информации о выбранном участнике
        /// </summary>
        /// <param name="seminarId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("{seminarId}/get/{userId}/start-data")]
        public async Task<ActionResult<SeminarMemberManagerRequestDto>> GetSeminarMemberManagerRequestStartData(long seminarId,
            long userId)
        {
            try
            {
                var member = await _seminarApplicationService.GetNewSeminarMemberManagerRequest(seminarId, userId);
                return Ok(member);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Сохраняет информацию об участниках семинара от руководителя
        /// </summary>
        /// <param name="seminarId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("{seminarId}/save/manager-request")]
        public async Task<IActionResult> SaveManagerRequest(long seminarId, [FromBody] SeminarMemberManagerRequestListDto request)
        {
            try
            {
                await _seminarApplicationService.CreateManagerMembersByClubAsync(seminarId, request);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Подтверждает готовность данных руководителя к отправке в итоговую ведомость
        /// </summary>
        /// <param name="seminarId"></param>
        /// <param name="managerId"></param>
        /// <param name="clubId"></param>
        /// <returns></returns>
        [HttpPut("{seminarId}/confirm-request/{managerId}/{clubId}")]
        public async Task<IActionResult> ConfirmManagerRequest(long seminarId, long managerId, long clubId)
        {
            try
            {
                await _seminarApplicationService.ConfirmManagerMembersByClubAsync(seminarId, managerId, clubId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Отменяет готовность данных руководителя к отправке в итоговую ведомость
        /// </summary>
        /// <param name="seminarId"></param>
        /// <param name="managerId"></param>
        /// <param name="clubId"></param>
        /// <returns></returns>
        [HttpPut("{seminarId}/cancel-request/{managerId}/{clubId}")]
        public async Task<IActionResult> CancelManagerRequest(long seminarId, long managerId, long clubId)
        {
            try
            {
                await _seminarApplicationService.CancelManagerMemberByClubAsync(seminarId, managerId, clubId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        #endregion

        #region SeminarMembers

        /// <summary>
        /// Получение всех заявок руководителей для итоговой ведомости
        /// </summary>
        /// <param name="seminarId"></param>
        /// <returns></returns>
        [HttpGet("{seminarId}/requested-members")]
        public async Task<ActionResult<List<SeminarMemberManagerRequestDto>>> GetManagerMembers(long seminarId)
        {
            try
            {
                var members = await _seminarApplicationService.GetAllManagerRequests(seminarId);
                return Ok(members);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Получения списка менеджеров и количества отправленных участников
        /// </summary>
        /// <returns></returns>
        [HttpGet("{seminarId}/manager-requests")]
        public async Task<ActionResult<List<ManagerRequest>>> GetManagerRequests(long seminarId)
        {
            try
            {
                var requests = await _seminarApplicationService.GetManagerRequestList(seminarId);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Устанавливает участников из заявок руководителей в итоговую ведомость
        /// </summary>
        /// <param name="seminarId"></param>
        /// <returns></returns>
        [HttpPost("{seminarId}/set/requested-members")]
        public async Task<IActionResult> SetRequestedMembers(long seminarId)
        {
            try
            {
                await _seminarApplicationService.SetRequestedMembers(seminarId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Получение всех участников семинара
        /// </summary>
        /// <param name="seminarId"></param>
        /// <returns></returns>
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
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Получение стартововой информации для ведомости выбранного участника (Возможно излишне, но пока оставлю)
        /// </summary>
        /// <param name="seminarId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("{seminarId}/get/member/start-data")]
        public async Task<ActionResult<SeminarMemberDto>> GetSeminarMemberStartData(long seminarId, long userId)
        {
            try
            {
                var member = await _seminarApplicationService.GetNewSeminarMember(seminarId, userId);
                return Ok(member);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Сохраняет список участников семинара
        /// </summary>
        /// <param name="seminarId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("{seminarId}/save/members")]
        public async Task<IActionResult> SaveSeminarMembers(long seminarId, [FromBody] SeminarMemberListDto request)
        {
            try
            {
                await _seminarApplicationService.SaveSeminarMembers(seminarId, request);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        #endregion
    }
}
