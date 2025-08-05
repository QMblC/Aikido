using Aikido.AdditionalData;
using Aikido.Dto;
using Aikido.Requests;
using Aikido.Services.DatabaseServices;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupController : Controller
    {
        private readonly UserDbService userService;
        private readonly ClubDbService clubService;
        private readonly GroupDbService groupService;
        private readonly ScheduleService scheduleService;

        public GroupController(
            UserDbService userService,
            ClubDbService clubService,
            GroupDbService groupService,
            ScheduleService scheduleService
            )
        {
            this.userService = userService;
            this.clubService = clubService;
            this.groupService = groupService;
            this.scheduleService = scheduleService;
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetGroupDataById(long id)
        {
            try
            {
                var group = await groupService.GetByIdOrThrowException(id);
                return Ok(new GroupDto(group));
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

        [HttpGet("get/list-by-club/{clubId}")]
        public async Task<IActionResult> GetGroupList(long clubId)
        {
            var groups = await groupService.GetGroupsByClubId(clubId);

            return Ok(groups.Select(group => new GroupDto(group)));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] GroupInfoRequest request)
        {
            GroupInfoDto groupData;

            try
            {
                groupData = await request.ParseGroupInfoAsync();
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при обработке JSON: {ex.Message}");
            }

            long groupId;

            try
            {
                await userService.GetByIdOrThrowException((long)groupData.CoachId);
            }
            catch
            {
                groupData.CoachId = null;
            }

            try
            {
                var groupDto = new GroupDto
                {
                    Name = groupData.Name,
                    AgeGroup = groupData.AgeGroup,
                    CoachId = groupData.CoachId,
                    ClubId = groupData.ClubId,
                    GroupMembers = groupData.GroupMembers?.Select(m => m.Id).ToList()
                };

                groupId = await groupService.CreateGroup(groupDto);
                await clubService.AddGroupToClub((long)groupData.ClubId, groupId);
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка создания группы", Details = ex.Message });
            }

            try
            {
                if (groupData.Schedule != null)
                {
                    await scheduleService.CreateGroupScheduleDeleteExcess(groupId, groupData.Schedule);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при создании расписания", Details = ex.Message });
            }

            try
            {
                var allDates = new List<ExclusionDateDto>();

                if (groupData.ExtraDates != null)
                {
                    allDates.AddRange(groupData.ExtraDates.Select(d => new ExclusionDateDto
                    {
                        GroupId = groupId,
                        Date = d,
                        Status = "Extra"
                    }));
                }

                if (groupData.MinorDates != null)
                {
                    allDates.AddRange(groupData.MinorDates.Select(d => new ExclusionDateDto
                    {
                        GroupId = groupId,
                        Date = d,
                        Status = "Minor"
                    }));
                }

                if (allDates.Any())
                {
                    await scheduleService.CreateExclusionDateDeleteExcess(groupId, allDates);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при сохранении исключений", Details = ex.Message });
            }

            return Ok(new { id = groupId });
        }



        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(long id, [FromForm] GroupInfoRequest request)
        {
            GroupInfoDto groupData;

            try
            {
                groupData = await request.ParseGroupInfoAsync();
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при обработке JSON группы: {ex.Message}");
            }

            try
            {
                var groupDto = new GroupDto
                {
                    Name = groupData.Name,
                    AgeGroup = groupData.AgeGroup,
                    CoachId = groupData.CoachId,
                    ClubId = groupData.ClubId,
                    GroupMembers = (groupData.GroupMembers != null && groupData.GroupMembers.Any())
                        ? groupData.GroupMembers.Select(m => m.Id).ToList()
                        : null
                };

                await groupService.UpdateGroup(id, groupDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка обновления группы", Details = ex.Message });
            }

            try
            {
                if (groupData.Schedule != null)
                {
                    await scheduleService.CreateGroupScheduleDeleteExcess(id, groupData.Schedule);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка обновления расписания", Details = ex.Message });
            }

            try
            {
                var exclusions = new List<ExclusionDateDto>();

                if (groupData.ExtraDates != null)
                {
                    exclusions.AddRange(groupData.ExtraDates.Select(date => new ExclusionDateDto
                    {
                        GroupId = id,
                        Date = date,
                        Status = "Extra"
                    }));
                }

                if (groupData.MinorDates != null)
                {
                    exclusions.AddRange(groupData.MinorDates.Select(date => new ExclusionDateDto
                    {
                        GroupId = id,
                        Date = date,
                        Status = "Minor"
                    }));
                }

                await scheduleService.CreateExclusionDateDeleteExcess(id, exclusions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при обновлении исключений", Details = ex.Message });
            }

            return Ok(new { Message = "Группа успешно обновлена", Id = id });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                await groupService.DeleteById(id);
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

        [HttpGet("get-by-club/{clubId}")]
        public async Task<IActionResult> GetGroupsByClubId(long clubId)
        {
            try
            {
                var groups = await groupService.GetGroupsByClubId(clubId);
                return Ok(groups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        [HttpGet("get/list")]
        public async Task<IActionResult> GetList()
        {
            var groups = await groupService.GetAll();

            return Ok(groups.Select(group => new GroupDto(group)));
        }

        [HttpGet("get/info/{groupId}")]
        public async Task<IActionResult> GetGroupInfo(long groupId)
        {
            var groupInfo = new GroupInfoDto();

            var group = await groupService.GetByIdOrThrowException(groupId);
            if (group == null)
                return NotFound(new { Message = $"Группа с Id = {groupId} не найдена." });

            groupInfo.Id = group.Id;
            groupInfo.Name = group.Name;
            groupInfo.AgeGroup = EnumParser.ConvertEnumToString(group.AgeGroup);
            groupInfo.ClubId = group.ClubId;

            if (group.ClubId != null)
            {
                try
                {
                    var club = await clubService.GetByIdOrThrowException((long)group.ClubId);
                    groupInfo.Club = club.Name;
                }
                catch (KeyNotFoundException)
                {
                    groupInfo.Club = null;
                }
            }
            else
            {
                groupInfo.Club = null;
            }

            groupInfo.CoachId = group.CoachId;

            if (group.CoachId != null)
            {
                try
                {
                    var coach = await userService.GetByIdOrThrowException((long)group.CoachId);
                    groupInfo.Coach = coach.FullName;
                }
                catch (KeyNotFoundException)
                {
                    groupInfo.Coach = null;
                }
            }
            else
            {
                groupInfo.Coach = null;
            }

            groupInfo.GroupMembers = new List<UserShortDto>();
            if (group.UserIds != null)
            {
                foreach (var userId in group.UserIds)
                {
                    try
                    {
                        var user = await userService.GetUserById(userId);
                        groupInfo.GroupMembers.Add(new UserShortDto
                        {
                            Id = userId,
                            Name = user.FullName
                        });
                    }
                    catch (KeyNotFoundException)
                    {
                        continue;
                    }
                }
            }

            var scheduleEntities = await scheduleService.GetGroupSchedule(groupId);
            var scheduleDict = new Dictionary<string, string>();
            foreach (var scheduleItem in scheduleEntities)
            {
                string dayKey = scheduleItem.DayOfWeek switch
                {
                    DayOfWeek.Monday => "Пн",
                    DayOfWeek.Tuesday => "Вт",
                    DayOfWeek.Wednesday => "Ср",
                    DayOfWeek.Thursday => "Чт",
                    DayOfWeek.Friday => "Пт",
                    DayOfWeek.Saturday => "Сб",
                    DayOfWeek.Sunday => "Вс",
                    _ => null
                };

                if (dayKey != null)
                {
                    scheduleDict[dayKey] = $"{scheduleItem.StartTime:hh\\:mm}-{scheduleItem.EndTime:hh\\:mm}";
                }
            }
            groupInfo.Schedule = scheduleDict;

            var exclusionDates = await scheduleService.GetGroupExclusionDates(groupId, DateTime.Now);
            groupInfo.ExtraDates = exclusionDates
                .Where(d => d.Status == ExclusiveDateType.Extra)
                .Select(d => d.Date.Date)
                .ToList();

            groupInfo.MinorDates = exclusionDates
                .Where(d => d.Status == ExclusiveDateType.Minor)
                .Select(d => d.Date.Date)
                .ToList();

            return Ok(groupInfo);
        }
    }
}
