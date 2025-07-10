using Aikido.Entities;
using Aikido.Requests;
using Aikido.Services;
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
            // Получаем группу
            var group = await groupService.GetGroupById(groupId);
            if (group == null)
                return NotFound($"Группа с Id = {groupId} не найдена.");

            // Парсим месяц в DateTime
            if (!DateTime.TryParseExact(month, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out var monthInDate))
                return BadRequest("Неверный формат месяца. Используйте 'yyyy-MM'.");

            // Список пользователей группы с посещаемостью
            var groupStudents = new List<object>();

            foreach (var userId in group.UserIds)
            {
                var user = await userService.GetUserById(userId);
                if (user == null)
                    continue;

                // Получаем посещаемость пользователя за месяц
                var attendance = await attendanceService.GetUserMonthlyAttendance(user.Id, monthInDate);

                groupStudents.Add(new
                {
                    User = user,
                    Attendance = attendance
                });
            }

            // Получаем расписание группы
            var schedule = await scheduleService.GetGroupSchedule(groupId);

            // Получаем даты исключений на месяц
            var exclusionDates = await scheduleService.GetGroupExclusionDates(groupId, monthInDate);

            // Формируем ответ
            var result = new
            {
                Group = group,
                Students = groupStudents,
                Schedule = schedule,
                ExclusionDates = exclusionDates
            };

            return Ok(result);
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
                return Ok(new { message = "Attendance added successfully" });
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
                return Ok(new { message = "Attendance removed successfully" });
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
