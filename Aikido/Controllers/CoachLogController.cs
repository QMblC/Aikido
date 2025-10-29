using Aikido.Application.Services;
using Aikido.Dto;
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
        public async Task<IActionResult> GetCoachGroups(long coachId)
        {
            try
            {
                var groups = await _groupApplicationService.GetAllGroupsAsync();
                var coachGroups = groups.Where(g => g.Coaches.Any(u => u.Id == coachId)).ToList();
                return Ok(coachGroups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении групп тренера", Details = ex.Message });
            }
        }

        [HttpGet("coach/{coachId}/students")]
        public async Task<IActionResult> GetCoachStudents(long coachId)
        {
            try
            {
                var allGroups = await _groupApplicationService.GetAllGroupsAsync();
                var coachGroups = allGroups
                    .Where(g => g.Coaches.Any(u => u.Id == coachId)).ToList();

                var allStudents = new List<UserShortDto>();

                foreach (var group in coachGroups)
                {
                    var groupMembers = await _groupApplicationService.GetGroupMembersAsync(group.Id.Value);
                    allStudents.AddRange(groupMembers);
                }

                var uniqueStudents = allStudents
                    .GroupBy(s => s.Id)
                    .Select(g => g.First())
                    .ToList();

                return Ok(uniqueStudents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении студентов тренера", Details = ex.Message });
            }
        }

        [HttpGet("coach/{coachId}/dashboard")]
        public async Task<IActionResult> GetCoachDashboard(long coachId)
        {
            try
            {
                var coach = await _userApplicationService.GetUserByIdAsync(coachId);
                var allGroups = await _groupApplicationService.GetAllGroupsAsync();
                var coachGroups = allGroups.Where(g => g.Coaches.Any(u => u.Id == coachId)).ToList();

                var totalStudents = 0;
                var groupsInfo = new List<object>();

                foreach (var group in coachGroups)
                {
                    var members = await _groupApplicationService.GetGroupMembersAsync(group.Id.Value);
                    totalStudents += members.Count;

                    groupsInfo.Add(new
                    {
                        GroupId = group.Id,
                        GroupName = group.Name,
                        MemberCount = members.Count,
                        MaxMembers = group.MaxMembers,
                        AgeGroup = group.AgeGroup
                    });
                }

                var dashboard = new
                {
                    Coach = new
                    {
                        Id = coach.Id,
                        Name = coach.FullName,
                        Grade = coach.Grade
                    },
                    TotalGroups = coachGroups.Count,
                    TotalStudents = totalStudents,
                    Groups = groupsInfo
                };

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении панели тренера", Details = ex.Message });
            }
        }

        [HttpPost("attendance")]
        public async Task<IActionResult> RecordAttendance([FromBody] List<AttendanceDto> attendanceList)
        {
            try
            {
                var createdIds = new List<long>();

                foreach (var attendance in attendanceList)
                {
                    var attendanceId = await _attendanceApplicationService.CreateAttendanceAsync(attendance);
                    createdIds.Add(attendanceId);
                }

                return Ok(new { Message = "Посещаемость записана", AttendanceIds = createdIds });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при записи посещаемости", Details = ex.Message });
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
