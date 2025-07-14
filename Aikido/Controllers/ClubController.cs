using Aikido.AdditionalData;
using Aikido.Dto;
using Aikido.Entities;
using Aikido.Requests;
using Aikido.Services;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClubController : Controller
    {
        private readonly UserService userService;
        private readonly ClubService clubService;
        private readonly GroupService groupService;
        private readonly ScheduleService scheduleService;

        public ClubController(
            UserService userService, 
            ClubService clubService, 
            GroupService groupService,
            ScheduleService scheduleService
            )
        {
            this.userService = userService;
            this.clubService = clubService;
            this.groupService = groupService;
            this.scheduleService = scheduleService;
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetClubDataById(long id)
        {
            try
            {
                var club = await clubService.GetClubById(id);
                return Ok(club);
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

        [HttpGet("get/details/{id}")]
        public async Task<IActionResult> GetClubDetails(long id)
        {
            ClubEntity club;

            try
            {
                club = await clubService.GetClubById(id);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }

            var result = await BuildClubDetailsDto(club);
            return Ok(result);
        }

        [HttpGet("get/details/list")]
        public async Task<IActionResult> GetAllClubDetails()
        {
            var clubs = await clubService.GetClubsList();
            var result = new List<ClubDetailsDto>();

            foreach (var club in clubs)
            {
                var dto = await BuildClubDetailsDto(club);
                result.Add(dto);
            }

            return Ok(result);
        }

        private async Task<ClubDetailsDto> BuildClubDetailsDto(ClubEntity club)
        {
            var groupEntities = await groupService.GetGroupsByClubId(club.Id);
            var groupDtos = new List<GroupDetailsDto>();

            foreach (var group in groupEntities)
            {
                UserEntity coach;
                try
                {
                    coach = await userService.GetUserById((long)group.CoachId);
                }
                catch
                {
                    coach = null;
                }

                var scheduleEntities = await scheduleService.GetGroupSchedule(group.Id);

                var scheduleDict = scheduleEntities
                    .GroupBy(s => s.DayOfWeek)
                    .ToDictionary(
                        g => DayOfWeekToRussian(g.Key),
                        g => string.Join("-", g.First().StartTime.ToString(@"hh\:mm"), g.First().EndTime.ToString(@"hh\:mm")),
                        StringComparer.OrdinalIgnoreCase);

                groupDtos.Add(new GroupDetailsDto
                {
                    Name = group.Name,
                    Coach = coach == null ? null : new CoachDto
                    {
                        Name = coach.FullName,
                        Grade = EnumParser.ConvertEnumToString(coach.Grade),
                        Phone = coach.PhoneNumber
                    },
                    Schedule = scheduleDict
                });
            }

            return new ClubDetailsDto
            {
                Id = club.Id,
                Name = club.Name,
                City = club.City,
                Address = club.Address,
                Groups = groupDtos
            };
        }

        private string DayOfWeekToRussian(DayOfWeek dayOfWeek) => dayOfWeek switch
        {
            DayOfWeek.Monday => "Пн",
            DayOfWeek.Tuesday => "Вт",
            DayOfWeek.Wednesday => "Ср",
            DayOfWeek.Thursday => "Чт",
            DayOfWeek.Friday => "Пт",
            DayOfWeek.Saturday => "Сб",
            DayOfWeek.Sunday => "Вс",
            _ => ""
        };

        [HttpGet("get/list")]
        public async Task<IActionResult> GetClubsList()
        {
            try
            {
                var clubs = await clubService.GetClubsList();
                return Ok(clubs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] ClubRequest request)
        {
            ClubDto clubData;

            try
            {
                clubData = await request.Parse();
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при обработке JSON: {ex.Message}");
            }

            long clubId;

            try
            {
                clubId = await clubService.CreateClub(clubData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }

            return Ok(new { id = clubId });
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(long id, [FromForm] ClubRequest request)
        {
            var updatedClub = await request.Parse();

            if (updatedClub == null)
            {
                return BadRequest("Данные клуба не переданы.");
            }

            try
            {
                await clubService.UpdateClub(id, updatedClub);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }

            return Ok(new { Message = "Клуб успешно обновлён." });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {   var groupsDelete = clubService.GetClubById(id).Result.Groups;

                foreach (var group in groupsDelete)
                {
                    await groupService.DeleteGroup(group, false);
                }

                await clubService.DeleteClub(id);
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

        
    }
}
