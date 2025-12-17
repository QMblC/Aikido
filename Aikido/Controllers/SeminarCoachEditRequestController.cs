using Aikido.Application.Services;
using Aikido.Dto.Seminars.Members;
using Aikido.Dto.Seminars.Members.CoachEditRequest;
using Aikido.Dto.Seminars.Members.Creation;
using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api")]
    public class SeminarCoachEditRequestController : ControllerBase
    {
        private readonly SeminarCoachEditRequestAppService _seminarCoachEditAppServiceAppService;
        private readonly SeminarApplicationService _seminarApplicationService;

        public SeminarCoachEditRequestController(
            SeminarCoachEditRequestAppService seminarCoachEditAppSerivce,
            SeminarApplicationService seminarAppService)
        {
            _seminarCoachEditAppServiceAppService = seminarCoachEditAppSerivce;
            _seminarApplicationService = seminarAppService;
        }

        /// <summary>
        /// Получение содержимого запроса тренера на редактирование ведомости руководителя
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        [HttpGet("seminar/coach-request/get/{requestId}/data")]
        public async Task<ActionResult<List<SeminarMemberRequestDto>>> GetRequestData(long requestId)
        {
            try
            {
                var requestData = await _seminarCoachEditAppServiceAppService.GetCoachRequestData(requestId);
                return Ok(requestData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Получение списка заявок тренера конкретного клуба
        /// </summary>
        /// <param name="seminarId"></param>
        /// <param name="clubId"></param>
        /// <param name="coachId"></param>
        /// <returns></returns>
        [HttpGet("seminar/{seminarId}/coach-request/club/{clubId}/coach/{coachId}")]
        public async Task<ActionResult<List<SeminarMemberRequestDto>>> GetCoachClubRequests(long seminarId, long clubId, long coachId)
        {
            try
            {
                var requests = await _seminarCoachEditAppServiceAppService.GetClubCoachRequestList(seminarId, clubId, coachId);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Получение всех заявок конкретного клуба для руководителя
        /// </summary>
        /// <param name="seminarId"></param>
        /// <param name="clubId"></param>
        /// <returns></returns>
        [HttpGet("seminar/{seminarId}/coach-request/club/{clubId}")]
        public async Task<ActionResult<List<SeminarMemberCoachRequestDto>>> GetCoachRequests(long seminarId, long clubId)
        {
            try
            {
                await _seminarCoachEditAppServiceAppService.GetCoachRequests(seminarId, clubId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Создаёт запрос тренера на редактирование ведомости руководителя
        /// </summary>
        /// <param name="seminarId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("seminar/{seminarId}/coach-request/create")]
        public async Task<IActionResult> CreateCoachRequest(long seminarId,
            [FromBody] SeminarMemberCoachRequestListCreationDto request)
        {
            try
            {
                await _seminarCoachEditAppServiceAppService.CreateCoachRequest(seminarId, request);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Обновление завяки тренером, после обновления статус автоматически меняется на Pending
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("seminar/{seminarId}/coach-request/update/{requestId}")]
        public async Task<IActionResult> UpdateRequestByCoach(long requestId,
            [FromBody] SeminarMemberCoachRequestListCreationDto request)
        {
            try
            {
                await _seminarCoachEditAppServiceAppService.UpdateRequestByCoach(requestId, request);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Удаляет заявку тренера
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        [HttpDelete("seminar/{seminarId}/coach-request/delete/{requestId}")]
        public async Task<IActionResult> DeleteCoachRequest(long requestId)
        {
            try
            {
                await _seminarCoachEditAppServiceAppService.DeleteCoachRequest(requestId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Применение изменений указанной заявки
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reviewerId"></param>
        /// <returns></returns>
        [HttpPut("seminar/coach-request/apply/{id}")]
        public async Task<IActionResult> ApplyRequest(long id, [FromQuery] long reviewerId)
        {
            try
            {
                await _seminarCoachEditAppServiceAppService.ApplyRequest(id, reviewerId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Отклонение изменений указанной заявки
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reviewerId"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        [HttpPut("seminar/coach-request/reject/{id}")]
        public async Task<IActionResult> RejectRequest(long id, [FromQuery] long reviewerId, [FromBody] string comment)
        {
            try
            {
                await _seminarCoachEditAppServiceAppService.RejectRequest(id, reviewerId, comment);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }
    }
}