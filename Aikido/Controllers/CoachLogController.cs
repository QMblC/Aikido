using Aikido.Entities;
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

        public CoachLogController(
            UserService userService,
            ClubService clubService,
            GroupService groupService,
            TableService tableService,
            ScheduleService scheduleService
            )
        {
            this.userService = userService;
            this.clubService = clubService;
            this.groupService = groupService;
            this.tableService = tableService;
            this.scheduleService = scheduleService;
        }

        [HttpGet("get/data/{groupId}/")]
        public async Task<IActionResult> GetData(long groupId, [FromQuery] string month)
        {
            var group = await groupService.GetGroupById(groupId);

            var groupStudents = new List<UserEntity>();

            foreach (var id in group.UserIds)
            {
                groupStudents.Add(await userService.GetUserById(id));
                //ToDo добавляй посещаемость к пользователю
            }

            var schedule = await scheduleService.GetGroupSchedule(groupId);

            var monthInDate = DateTime.ParseExact(month, "yyyy-MM", null);

            var exclusionDates = await scheduleService.GetGroupExclusionDates(groupId, monthInDate);

            return Ok();
        }

        [HttpPost("add-attendance")]
        public async Task<IActionResult> AddAttendance([FromForm] object o)
        {
            return Ok();
        }

        [HttpDelete("remove-attendance")]
        public async Task<IActionResult> RemoveAttendance([FromForm] object o)
        {
            return Ok();
        }

    }
}
