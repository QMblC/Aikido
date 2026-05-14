using Aikido.Application.Services;
using Aikido.Dto.Attendance;
using Aikido.Dto.Groups;
using Aikido.Dto.Users;
using Aikido.Services;
using Aikido.Services.ApplicationServices.Attendnace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoachLogController : ControllerBase
    {
        private readonly UserApplicationService _userApplicationService;
        private readonly GroupApplicationService _groupApplicationService;
        private readonly IAttendanceApplicationService _attendanceApplicationService;
        private readonly TableService _tableService;

        public CoachLogController(
            UserApplicationService userApplicationService,
            GroupApplicationService groupApplicationService,
            IAttendanceApplicationService attendanceApplicationService,
            TableService tableService)
        {
            _userApplicationService = userApplicationService;
            _groupApplicationService = groupApplicationService;
            _attendanceApplicationService = attendanceApplicationService;
            _tableService = tableService;
        }

        /// <summary>
        /// Получение групп пользователя
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("user/{userId}/groups")]
        public async Task<ActionResult<List<GroupShortDto>>> GetUserGroups(long userId)
        {
            try
            {
                var userGroups = await _groupApplicationService.GetGroupsByUserAsync(userId);
                return Ok(userGroups
                    .Where(ug => !ug.Coaches.Any(u => u.Id == userId))
                    .ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении групп тренера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Получение групп тренера
        /// </summary>
        /// <param name="coachId"></param>
        /// <returns></returns>
        [HttpGet("coach/{coachId}/groups")]
        public async Task<ActionResult<List<GroupShortDto>>> GetCoachGroups(long coachId)
        {
            try
            {
                var coachGroups = await _groupApplicationService.GetGroupsByCoach(coachId);
                return Ok(coachGroups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении групп тренера", Details = ex.Message });
            }
        }
        
        /// <summary>
        /// Получение панели тренера конкретной группы
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager,Coach")]
        [HttpGet("get/{groupId}/monthly-attendance")]
        public async Task<ActionResult<GroupDashboardDto>> GetCoachDashboard(long groupId, [FromQuery] DateTime month)
        {
            try
            {
                var dashboard = await _groupApplicationService.GetGroupDashboard(groupId, month);

                return Ok(dashboard);
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении панели тренера", Details = ex.Message });
            }  
        }

        /// <summary>
        /// Получение панели тренера конкретной группы в виде Excel
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager,Coach")]
        [HttpGet("get/{groupId}/monthly-attendance/table")]
        public async Task<IActionResult> GetAttendanceTable(long groupId, [FromQuery] DateTime month)
        {
            try
            {
                var dashboard = await _groupApplicationService.GetGroupDashboard(groupId, month);

                var tableStream = _tableService.GetAttendanceTable(dashboard, month.Year, month.Month);

                return File(tableStream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"{month.Month}-{month.Year} Posecshaemost.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении таблицы посещаемости", Details = ex.Message });
            }
        }

        /// <summary>
        /// Получение посещений ученика
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("get/{groupId}/user/{userId}/monthly-attendance")]
        public async Task<ActionResult<List<GroupDashboardDto>>> GetUserAttendance(long groupId, long userId, [FromQuery] DateTime month)
        {
            try
            {
                var userDashboard = await _groupApplicationService.GetUserDashboard(groupId, userId, month);

                return Ok(userDashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении панели пользователя", Details = ex.Message });
            }
        }

        /// <summary>
        /// Создание одного посещения
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="attendance"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager,Coach")]
        [HttpPost("create/{groupId}/attendance")]
        public async Task<IActionResult> CreateSingleAttendance(long groupId, [FromBody] AttendanceCreationDto attendance)
        {
            try
            {
                var attendanceId = await _attendanceApplicationService.MarkAttendanceAsync(groupId, attendance);

                return Ok(attendanceId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при записи посещаемости", Details = ex.Message });
            }
        }

        /// <summary>
        /// Удаление одного посещения
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager,Coach")]
        [HttpDelete("delete/attendance/{id}")]
        public async Task<IActionResult> DeleteAttendance(long id)
        {
            try
            {
                await _attendanceApplicationService.DeleteAttendanceAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при удалении посещаемости", Details = ex.Message });
            }
        }

        /// <summary>
        /// Обновление посещений списком
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="attendances"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager,Coach")]
        [HttpPut("update/{groupId}/attendance")]
        public async Task<IActionResult> UpdateAttendances(long groupId, [FromBody] AttendanceUpdateDto attendances)
        {
            try
            {
                await _attendanceApplicationService.UpdateAttendances(groupId, attendances);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при записи посещаемости", Details = ex.Message });
            }
        }
    }
}
