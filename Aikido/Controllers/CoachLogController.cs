using Aikido.AdditionalData;
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

        [HttpGet("get/groups-by-user/{userId}")]
        public async Task<IActionResult> GetGroupsByUser(long userId)
        {
            var groups = await groupService.GetGroupsByUser(userId);

            return Ok(groups.Select(group => new GroupDto(group)));
        }

        [HttpGet("get/data/student")]
        public async Task<IActionResult> GetUserAttendance(
            [FromQuery] long userId,
            [FromQuery] long groupId,
            [FromQuery] string month)
        {
            if (!DateTime.TryParseExact(month, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out var monthInDate))
                return BadRequest("Неверный формат месяца. Используйте 'yyyy-MM'.");

            var user = await userService.GetUserById(userId);
            if (user == null)
                return NotFound($"Пользователь с Id = {userId} не найден.");

            var attendance = await attendanceService.GetUserMonthlyAttendance(user.Id, groupId, monthInDate);

            var scheduleRaw = await scheduleService.GetGroupSchedule(groupId);

            var localizedSchedule = scheduleRaw
                .GroupBy(x => x.DayOfWeek)
                .ToDictionary(
                    g => GetDayOfWeekShortName((int)g.Key),
                    g => string.Join(", ", g.Select(s => $"{s.StartTime:hh\\:mm}-{s.EndTime:hh\\:mm}"))
                );

            var group = await groupService.GetGroupById(groupId);

            ClubEntity club;
            if (user.ClubId != null)
            {
                club = await clubService.GetClubById(user.ClubId.Value);
            }
            else
            {
                throw new NotImplementedException("У пользователя не привязан клуб");
            }

            UserShortDto? coach = null;

            if (group.CoachId != null)
            {
                var coachEntity = await userService.GetUserById(group.CoachId.Value);
                if (coachEntity != null)
                {
                    coach = new UserShortDto
                    {
                        Id = coachEntity.Id,
                        Name = coachEntity.FullName,
                        Photo = Convert.ToBase64String(coachEntity.Photo)
                    };
                }
            }

            var result = new
            {
                Group = new GroupDto(group),
                Coach = coach,
                Club = new ClubDto(club),
                Attendance = attendance,
                Schedule = localizedSchedule
            };

            return Ok(result);
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

                var attendance = await attendanceService.GetUserMonthlyAttendance(user.Id, groupId, monthInDate);

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

            var scheduleRaw = await scheduleService.GetGroupSchedule(groupId);

            var localizedSchedule = scheduleRaw
                .GroupBy(x => x.DayOfWeek)
                .ToDictionary(
                    g => GetDayOfWeekShortName((int)g.Key),
                    g => string.Join(", ", g.Select(s => $"{s.StartTime:hh\\:mm}-{s.EndTime:hh\\:mm}"))
                );

            var exclusionDates = await scheduleService.GetGroupExclusionDates(groupId, monthInDate);

            var result = new
            {
                Group = new GroupDto(group),
                Students = groupStudents,
                Schedule = localizedSchedule,
                ExtraDates = exclusionDates.Where(x => x.Status == ExclusiveDateType.Extra),
                MinorDates = exclusionDates.Where(x => x.Status == ExclusiveDateType.Minor)
            };

            return Ok(result);
        }

        private static string GetDayOfWeekShortName(int dayOfWeek)
        {
            return dayOfWeek switch
            {
                0 => "Вс",
                1 => "Пн",
                2 => "Вт",
                3 => "Ср",
                4 => "Чт",
                5 => "Пт",
                6 => "Сб",
                _ => "?"
            };
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
