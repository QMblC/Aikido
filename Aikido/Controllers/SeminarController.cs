using Aikido.AdditionalData.Enums;
using Aikido.Application.Services;
using Aikido.Dto;
using Aikido.Dto.Seminars;
using Aikido.Dto.Seminars.Creation;
using Aikido.Dto.Seminars.Members;
using Aikido.Dto.Seminars.Members.Creation;
using Aikido.Dto.Users;
using Aikido.Entities.Seminar.SeminarFilters;
using Aikido.Exceptions;
using Aikido.Requests;
using Aikido.Services;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
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

        /// <summary>
        /// Получение информации о конкретном семинаре
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Получение списка всех семинаров
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet("get/all")]
        public async Task<ActionResult<List<SeminarShortDto>>> GetAllSeminars([FromQuery] TimeFilter filter)
        {
            try
            {
                var seminars = await _seminarApplicationService.GetAllSeminarsAsync(filter);
                return Ok(seminars);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении списка семинаров", Details = ex.Message });
            }
        }

        /// <summary>
        /// Создание семинаров (Админ)
        /// </summary>
        /// <param name="seminarData"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateSeminar([FromBody] SeminarCreationDto seminarData)
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

        /// <summary>
        /// Обновление семинара
        /// </summary>
        /// <param name="id"></param>
        /// <param name="seminarData"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateSeminar(long id, [FromBody] SeminarCreationDto seminarData)
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

        /// <summary>
        /// Удаление семинара (Админ)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
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
        /// <summary>
        /// Получение положения семинара
        /// </summary>
        /// <param name="seminarId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Создание положения семинара
        /// </summary>
        /// <param name="seminarId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager")]
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

        /// <summary>
        /// Удаление положения семинара
        /// </summary>
        /// <param name="seminarId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager")]
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

        #region ManagerRequests

        /// <summary>
        /// Получение списка участников семинара конкретного руководителя
        /// </summary>
        /// <param name="seminarId"></param>
        /// <param name="managerId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager,Coach")]
        [HttpGet("{seminarId}/requested-members/manager/{managerId}")]
        public async Task<ActionResult<List<SeminarMemberRequestDto>>> GetSeminarMembersManagerRequest(long seminarId,
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
        /// <param name="clubId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager,Coach")]
        [HttpGet("{seminarId}/requested-members/club/{clubId}")]
        public async Task<ActionResult<List<SeminarMemberRequestDto>>> GetClubSeminarMembersManagerRequest(long seminarId,
            long clubId)
        {
            try
            {
                var members = await _seminarApplicationService.GetClubRequestedMembers(seminarId, clubId);
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
        [Authorize(Roles = "Admin,Manager,Coach")]
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
        /// Можно использовать и для руководителей и для тренеров
        /// </summary>
        /// <param name="seminarId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager,Coach")]
        [HttpGet("{seminarId}/get/{userId}/start-data")]
        public async Task<ActionResult<SeminarMemberRequestDto>> GetSeminarMemberManagerRequestStartData(long seminarId,
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
        [Authorize(Roles = "Admin,Manager")]
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
        /// Подтверждена ли ведомость клуба
        /// </summary>
        /// <param name="seminarId"></param>
        /// <param name="clubId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("{seminarId}/requested-members/club/{clubId}/is-confirmed")]
        public async Task<ActionResult<bool>> IsClubSeminarMembersManagerRequestConfirmed(long seminarId,
            long clubId)
        {
            try
            {
                var isConfirmed = await _seminarApplicationService.IsClubSeminarMembersManagerRequestConfirmed(seminarId, clubId);
                return Ok(isConfirmed);
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
        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("{seminarId}/confirm-request/{managerId}/{clubId}")]
        public async Task<IActionResult> ConfirmManagerRequest(long seminarId, long managerId, long clubId)
        {
            try
            {
                await _seminarApplicationService.ConfirmManagerMembersByClubAsync(seminarId, managerId, clubId);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(422, new { Message = "Невозможная операция", Details = ex.Message });
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
        [Authorize(Roles = "Admin,Manager")]
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
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("{seminarId}/requested-members")]
        public async Task<ActionResult<List<SeminarMemberRequestDto>>> GetManagerMembers(long seminarId)
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
        /// Получение списка менеджеров и количества отправленных участников
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager")]
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
        [Authorize(Roles = "Admin,Manager")]
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
        [Authorize]
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
        /// Получение стартововой информации для ведомости выбранного участника (Для финальной)
        /// </summary>
        /// <param name="seminarId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager,Coach")]
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
        [Authorize(Roles = "Admin,Manager")]
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

        #region CoachRequests

        /// <summary>
        /// Получение участников указанного тренера по клубам
        /// </summary>
        /// <param name="seminarId"></param>
        /// <param name="clubId"></param>
        /// <param name="coachId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager,Coach")]
        [HttpGet("coach/{coachId}/club/{clubId}/members")]
        public async Task<ActionResult<List<SeminarMemberRequestDto>>> GetCoachMembersByClub(long seminarId,
            long clubId,
            long coachId)
        {
            try
            {
                var members = await _seminarApplicationService.GetCoachMembersByClub(seminarId, clubId, coachId);
                return Ok(members);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
            
        }

        /// <summary>
        /// Поиск по имени участников клуба, которых тренерует тренер
        /// </summary>
        /// <param name="coachId"></param>
        /// <param name="clubId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager,Coach")]
        [HttpGet("coach/{coachId}/club/{clubId}")]
        public async Task<ActionResult<UserShortDto>> FindCoachMemberByClub(long coachId,
            long clubId,
            [FromQuery] string name)
        {
            try
            {
                var members = await _seminarApplicationService.FindCoachMemberInClubByName(clubId, coachId, name);
                return Ok(members);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
            
        }

        /// <summary>
        /// Таблица с участниками семинара конкретного клуба
        /// </summary>
        /// <param name="seminarId"></param>
        /// <param name="clubId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager,Coach")]
        [HttpGet("{seminarId}/requested-members/club/{clubId}/table")]
        public async Task<IActionResult> GetClubSeminarMembersManagerRequestTable(long seminarId,
            long clubId)
        {
            try
            {
                var members = await _seminarApplicationService.GetClubRequestedMembers(seminarId, clubId);

                if (members == null)
                {
                    return NotFound($"Не найдены участники клуба с Id = {clubId}");
                }

                var stream = _tableService.CreateSeminarMembersTable(members.Cast<ISeminarMemberDataDto>().ToList());
                return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Members.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Таблица с участниками семинара конкретного клуба
        /// </summary>
        /// <param name="seminarId"></param>
        /// <param name="managerId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager,Coach")]
        [HttpGet("{seminarId}/requested-members/manager/{managerId}/table")]
        public async Task<IActionResult> GetSeminarMembersManagerRequestTable(long seminarId,
            long managerId)
        {
            try
            {
                var members = await _seminarApplicationService.GetRequestedMembers(seminarId, managerId);

                if (members == null)
                {
                    return NotFound($"Не найдены участники руководителя с Id = {managerId}");
                }

                var stream = _tableService.CreateSeminarMembersTable(members.Cast<ISeminarMemberDataDto>().ToList());
                return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Members.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Таблица с участниками семинара от руководителей
        /// </summary>
        /// <param name="seminarId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager,Coach")]
        [HttpGet("{seminarId}/requested-members/table")]
        public async Task<IActionResult> GetManagerMembersTable(long seminarId)
        {
            try
            {
                var members = await _seminarApplicationService.GetAllManagerRequests(seminarId);

                if (members == null)
                {
                    return NotFound($"Не найдены участники");
                }

                var stream = _tableService.CreateSeminarMembersTable(members.Cast<ISeminarMemberDataDto>().ToList());
                return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Members.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Загрузка итоговой ведомости
        /// </summary>
        /// <param name="seminarId"></param>
        /// <returns></returns>
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

                var stream = _tableService.CreateSeminarMembersTable(members.Cast<ISeminarMemberDataDto>().ToList());
                return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Members.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Загрузка списка финальной ведомости
        /// </summary>
        /// <param name="seminarId"></param>
        /// <param name="tableFile"></param>
        /// <returns></returns>
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
                        //var creationMember = new FinalSeminarMemberDto()
                        //{
                        //    UserId = m.UserId,
                        //    Status = m.CertificationGrade != null
                        //    ? EnumParser.ConvertEnumToString(SeminarMemberStatus.Certified)
                        //    : EnumParser.ConvertEnumToString(SeminarMemberStatus.Training),
                        //    CertificationGrade = m.CertificationGrade,
                        //    SeminarPriceInRubles = m.SeminarPriceInRubles,
                        //    BudoPassportPriceInRubles = m.BudoPassportPriceInRubles,
                        //    AnnualFeePriceInRubles = m.AnnualFeePriceInRubles,
                        //    CertificationPriceInRubles = m.CertificationPriceInRubles

                        //};
                        //return creationMember;
                    });

                    //var members = await Task.WhenAll(fullCreationDataTasks);
                    //await _seminarApplicationService.SaveSeminarMembers(seminarId, members.ToList());
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }


        #endregion

        /// <summary>
        /// Применяет результаты семинара
        /// </summary>
        /// <param name="seminarId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager")]
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
        [Authorize(Roles = "Admin,Manager")]
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

        /// <summary>
        /// Блокировка ведомостей семинара
        /// </summary>
        /// <param name="seminarId"></param>
        /// <returns></returns>
        [HttpPatch("{seminarId}/block")]
        public async Task<IActionResult> BlockSeminarStatements(long seminarId)
        {
            try
            {
                await _seminarApplicationService.BlockSeminarStatements(seminarId);
                return NoContent();
            }
            catch(EntityNotFoundException ex)
            {
                return NotFound(new {Message = "Объект не найден", Details = ex.Message});
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Разблокировка ведомостей семинара
        /// </summary>
        /// <param name="seminarId"></param>
        /// <returns></returns>
        [HttpPatch("{seminarId}/unlock")]
        public async Task<IActionResult> UnlockSeminarStatements(long seminarId)
        {
            try
            {
                await _seminarApplicationService.UnlockSeminarStatements(seminarId);
                return NoContent();
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { Message = "Объект не найден", Details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }
    }
}
