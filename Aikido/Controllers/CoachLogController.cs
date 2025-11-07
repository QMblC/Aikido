using Aikido.Application.Services;
using Aikido.Dto.Attendance;
using Aikido.Dto.Groups;
using Aikido.Dto.Users;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoachLogController : ControllerBase
    {
        private readonly UserApplicationService _userApplicationService;
        private readonly GroupApplicationService _groupApplicationService;
        private readonly AttendanceApplicationService _attendanceApplicationService;

        public CoachLogController(
            UserApplicationService userApplicationService,
            GroupApplicationService groupApplicationService,
            AttendanceApplicationService attendanceApplicationService)
        {
            _userApplicationService = userApplicationService;
            _groupApplicationService = groupApplicationService;
            _attendanceApplicationService = attendanceApplicationService;
        }

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


        [HttpGet("get/{groupId}/monthly-attendance")]
        public async Task<ActionResult<GroupDashboardDto>> GetCoachDashboard(long groupId, [FromQuery] DateTime month)
        {
            try
            {
                var group = await _groupApplicationService.GetGroupByIdAsync(groupId);
                var groupMembers = await _groupApplicationService.GetGroupMembersAsync(groupId);
                var attendances = await _attendanceApplicationService.GetAttendanceByGroup(groupId, month);

                return new GroupDashboardDto(group, groupMembers, attendances);
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении панели тренера", Details = ex.Message });
            }  
        }

        [HttpPost("attendance")]
        public async Task<IActionResult> RecordAttendance(
            long groupId,
            [FromBody] List<AttendanceCreationDto> attendanceList)
        {
            try
            {
                var createdIds = new List<long>();

                foreach (var attendance in attendanceList)
                {
                    var attendanceId = await _attendanceApplicationService.CreateAttendanceAsync(groupId, attendance);
                    createdIds.Add(attendanceId);
                }

                return Ok(new { Message = "Посещаемость записана", AttendanceIds = createdIds });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при записи посещаемости", Details = ex.Message });
            }
        }

        [HttpPost("create/{groupId}/attendance")]
        public async Task<IActionResult> CreateSingleAttendance(long groupId, [FromBody] AttendanceCreationDto attendance)
        {
            try
            {
                var attendanceId = await _attendanceApplicationService.CreateAttendanceAsync(groupId, attendance);

                return Ok(attendanceId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при записи посещаемости", Details = ex.Message });
            }
        }

        [HttpDelete("delete/attendance/{id}")]
        public async Task<IActionResult> DeleteAttendance(long id)
        {
            try
            {
                await _attendanceApplicationService.DeleteAttendanceAsync(id);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при удалении посещаемости", Details = ex.Message });
            }
        }

        [HttpGet("group/{groupId}/attendance-history")]
        public async Task<IActionResult> GetGroupAttendanceHistory(long groupId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                // Здесь нужно будет реализовать получение истории посещаемости группы
                // Пока что возвращаем заглушку
                return Ok(new { Message = "История посещаемости группы будет реализована" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении истории посещаемости", Details = ex.Message });
            }
        }
    }
}
