using Aikido.Application.Services;
using Aikido.Dto.Seminars.Members;
using Aikido.Dto.Seminars.Members.TrainerEditRequest;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/{seminarId}")]
    public class SeminarTrainerEditRequestController : ControllerBase
    {
        private readonly SeminarTrainerEditRequestAppService _seminarCoachEditAppServiceAppService;
        private readonly SeminarApplicationService _seminarApplicationService;

        public SeminarTrainerEditRequestController(
            SeminarTrainerEditRequestAppService seminarCoachEditAppSerivce,
            SeminarApplicationService seminarAppService)
        {
            _seminarCoachEditAppServiceAppService = seminarCoachEditAppSerivce;
            _seminarApplicationService = seminarAppService;
        }

        /// <summary>
        /// Получение участников указанного тренера по клубам
        /// </summary>
        /// <param name="seminarId"></param>
        /// <param name="clubId"></param>
        /// <param name="coachId"></param>
        /// <returns></returns>
        [HttpGet("coach/{coachId}/club/{clubId}/members")]
        public async Task<ActionResult<List<SeminarMemberManagerRequestDto>>> GetCoachMembersByClub(long seminarId,
            long clubId,
            long coachId)
        {
            var members = await _seminarApplicationService.GetCoachMembersByClub(seminarId, clubId, coachId);
            return Ok(members);
        }

        /// <summary>
        /// Поиск по имени участников клуба, которых тренерует тренер
        /// </summary>
        /// <param name="coachId"></param>
        /// <param name="clubId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet("coach/{coachId}/club/{clubId}")]
        public async Task<ActionResult<SeminarMemberManagerRequestDto>> FindCoachMemberByClub(long coachId, 
            long clubId, 
            [FromQuery] string name)
        {
            var members = await _seminarApplicationService.FindCoachMemberInClubByName(clubId, coachId, name);
            return Ok(members);
        }

        /// <summary>
        /// Тренер получает свои заявки по конкретному клубу
        /// GET: api/seminar/{seminarId}/trainer-requests/trainer/{trainerId}/club/{clubId}
        /// </summary>
        [HttpGet("coach/{trainerId}/club/{clubId}/requests")]
        public async Task<ActionResult<List<SeminarMemberTrainerEditRequestDto>>>
            GetTrainerRequestsByClub(long seminarId, long trainerId, long clubId)
        {
            var requests = await _seminarCoachEditAppServiceAppService.GetTrainerRequestsByClubAsync(
                seminarId, trainerId, clubId);
            return Ok(requests);
        }

        /// <summary>
        /// Тренер получает все свои заявки по всем клубам
        /// GET: api/seminar/{seminarId}/trainer-requests/trainer/{trainerId}
        /// </summary>
        [HttpGet("trainer/{trainerId}")]
        public async Task<ActionResult<List<SeminarMemberTrainerEditRequestDto>>>
            GetTrainerAllRequests(long seminarId, long trainerId)
        {
            var requests = await _seminarCoachEditAppServiceAppService.GetTrainerAllRequestsAsync(seminarId, trainerId);
            return Ok(requests);
        }

        /// <summary>
        /// Менеджер получает необработанные заявки по клубу
        /// GET: api/seminar/{seminarId}/trainer-requests/manager/{managerId}/club/{clubId}
        /// </summary>
        [HttpGet("manager/{managerId}/club/{clubId}")]
        public async Task<ActionResult<List<SeminarMemberTrainerEditRequestDto>>>
            GetManagerRequestsByClub(long seminarId, long managerId, long clubId)
        {
            var requests = await _seminarCoachEditAppServiceAppService.GetManagerRequestsByClubAsync(
                seminarId, managerId, clubId);
            return Ok(requests);
        }

        /// <summary>
        /// Менеджер получает все необработанные заявки по семинару
        /// GET: api/seminar/{seminarId}/trainer-requests/pending
        /// </summary>
        [HttpGet("pending")]
        public async Task<ActionResult<List<SeminarMemberTrainerEditRequestDto>>>
            GetPendingRequests(long seminarId)
        {
            var requests = await _seminarCoachEditAppServiceAppService.GetPendingRequestsAsync(seminarId);
            return Ok(requests);
        }

        /// <summary>
        /// Менеджер получает необработанные заявки, сгруппированные по тренерам и клубам
        /// GET: api/seminar/{seminarId}/trainer-requests/pending/grouped
        /// </summary>
        [HttpGet("pending/grouped")]
        public async Task<ActionResult<List<TrainerRequestGroupDto>>>
            GetGroupedPendingRequests(long seminarId)
        {
            var requests = await _seminarCoachEditAppServiceAppService.GetGroupedPendingRequestsAsync(seminarId);
            return Ok(requests);
        }

        /// <summary>
        /// Получить одну заявку с деталями
        /// GET: api/seminar/{seminarId}/trainer-requests/{requestId}
        /// </summary>
        [HttpGet("{requestId}")]
        public async Task<ActionResult<SeminarMemberTrainerEditRequestDto>>
            GetRequestDetails(long seminarId, long requestId)
        {
            var request = await _seminarCoachEditAppServiceAppService.GetRequestDetailsAsync(requestId);
            return Ok(request);
        }

        /// <summary>
        /// Получить статистику заявок тренера
        /// GET: api/seminar/{seminarId}/trainer-requests/stats/trainer/{trainerId}
        /// </summary>
        [HttpGet("stats/trainer/{trainerId}")]
        public async Task<ActionResult<TrainerRequestStatsDto>>
            GetTrainerStats(long seminarId, long trainerId)
        {
            var stats = await _seminarCoachEditAppServiceAppService.GetTrainerStatsAsync(seminarId, trainerId);
            return Ok(stats);
        }

        /// <summary>
        /// Тренер отправляет заявки по клубу
        /// 
        /// Тренер отправляет ПОЛНЫЙ список участников для этого клуба.
        /// Система автоматически определит:
        /// - Кого добавить (новые члены)
        /// - Кого обновить (изменились данные)
        /// - Кого удалить (больше не нужны)
        /// 
        /// POST: api/seminar/{seminarId}/trainer-requests/send
        /// </summary>
        [HttpPost("send")]
        public async Task<ActionResult> SendTrainerRequests(
            long seminarId,
            [FromBody] SeminarMemberTrainerEditRequestListDto requestList)
        {
            if (requestList == null)
                return BadRequest("Список заявок не может быть null");

            await _seminarCoachEditAppServiceAppService.SendTrainerRequestsByClubAsync(requestList);
            return Ok(new { message = "Заявки успешно отправлены" });
        }

        /// <summary>
        /// Менеджер одобряет или отклоняет заявку
        /// POST: api/seminar/{seminarId}/trainer-requests/{requestId}/review
        /// </summary>
        [HttpPost("{requestId}/review")]
        public async Task<ActionResult> ReviewRequest(
            long seminarId,
            long requestId,
            [FromBody] SeminarMemberTrainerEditRequestReviewDto reviewDto)
        {
            if (reviewDto == null)
                return BadRequest("Review данные не могут быть null");

            await _seminarCoachEditAppServiceAppService.ReviewRequestAsync(requestId, reviewDto);

            var status = reviewDto.IsApproved ? "одобрена" : "отклонена";
            return Ok(new { message = $"Заявка успешно {status}" });
        }

        /// <summary>
        /// Менеджер одобряет все заявки от одного тренера по одному клубу
        /// POST: api/seminar/{seminarId}/trainer-requests/trainer/{trainerId}/club/{clubId}/approve-all
        /// </summary>
        [HttpPost("trainer/{trainerId}/club/{clubId}/approve-all")]
        public async Task<ActionResult> ApproveAllTrainerClubRequests(
            long seminarId, long trainerId, long clubId)
        {
            await _seminarCoachEditAppServiceAppService.ApproveTrainerClubRequestsAsync(seminarId, trainerId, clubId);
            return Ok(new { message = "Все заявки успешно одобрены и применены" });
        }
    }
}