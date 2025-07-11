using Aikido.Dto;
using Aikido.Entities;
using Aikido.Requests;
using Aikido.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoachLogController : ControllerBase
    {
        private readonly UserService userService;
        private readonly ClubService clubService;
        private readonly GroupService groupService;
        private readonly TableService tableService;
        private readonly ScheduleService scheduleService;
        private readonly AttendanceService attendanceService;

        public CoachLogController(
            UserService userService,
            ClubService clubService,
            GroupService groupService,
            TableService tableService,
            ScheduleService scheduleService,
            AttendanceService attendanceService
            )
        {
            this.userService = userService;
            this.clubService = clubService;
            this.groupService = groupService;
            this.tableService = tableService;
            this.scheduleService = scheduleService;
            this.attendanceService = attendanceService;
        }

        [HttpGet("get/data/{groupId}/")]
        public async Task<IActionResult> GetData(long groupId, [FromQuery] string month)
        {
            var group = await groupService.GetGroupById(groupId);
            if (group == null)
                return NotFound($"Группа с Id = {groupId} не найдена.");

            if (!DateTime.TryParseExact(month, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out var monthInDate))
                return BadRequest("Неверный формат месяца. Используйте 'yyyy-MM'.");

            var groupStudents = new List<object>();

            foreach (var userId in group.UserIds)
            {
                var user = await userService.GetUserById(userId);
                if (user == null)
                    continue;

                var attendance = await attendanceService.GetUserMonthlyAttendance(user.Id, monthInDate);

                groupStudents.Add(new
                {
                    User = new UserShortDto
                    {
                        Id = user.Id,
                        Name = user.FullName,
                        Photo = Convert.ToBase64String(user.Photo)
                    },
                    Attendance = attendance
                });
            }

            var schedule = await scheduleService.GetGroupSchedule(groupId);

            var exclusionDates = await scheduleService.GetGroupExclusionDates(groupId, monthInDate);

            var result = new
            {
                Group = group,
                Students = groupStudents,
                Schedule = schedule,
                ExtraDates = exclusionDates.Where(x => x.Status == "extra"),
                MinorDates = exclusionDates.Where(x => x.Status == "minor")
            };

            return Ok(result);
        }

        [HttpPut("update/group-members/{groupId}")]
        public async Task<IActionResult> UpdateGroupMembers(long groupId, [FromBody] List<long> UserIds)
        {
            try
            {
                var users = new List<UserEntity>();

                foreach (var id in UserIds)
                {
                    var user = await userService.GetUserById(id);
                    if (user == null)
                        return NotFound($"Пользователь с ID {id} не найден.");

                    users.Add(user);
                }

                if (users.Any(u => u == null))
                    return NotFound("Один или несколько пользователей не найдены.");

                await groupService.UpdateGroupMembers(groupId, UserIds);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("get/group-members/{groupId}")]
        public async Task<IActionResult> GetGroupMembers(long groupId)
        {
            var memberIds = await groupService.GetGroupMemberIds(groupId);

            var users = new List<UserShortDto>();

            foreach (var userId in memberIds)
            {
                var user = await userService.GetUserById(userId);

                users.Add(new UserShortDto
                {
                    Id = user.Id,
                    Name = user.FullName,
                    Photo = Convert.ToBase64String(user.Photo)
                });
            }

            return Ok(users);
        }


        [HttpGet("get/groups-by-club/{clubId}")]
        public async Task<IActionResult> GetGroupsByClub(long clubId)
        {   
            var groups = await groupService.GetGroupsByClubId(clubId);

            return Ok(groups);
        }

        [HttpPost("add-attendance")]
        public async Task<IActionResult> AddAttendance([FromForm] AttendanceRequest request)
        {
            try
            {
                var attendanceDto = await request.Parse();
                await attendanceService.AddAttendance(attendanceDto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("remove-attendance")]
        public async Task<IActionResult> RemoveAttendance([FromForm] AttendanceRequest request)
        {
            try
            {
                var attendanceDto = await request.Parse();
                await attendanceService.RemoveAttendance(attendanceDto);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


    }
}
