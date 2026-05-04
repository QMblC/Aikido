using Aikido.AdditionalData.Enums;
using Aikido.Application.Services;
using Aikido.Dto;
using Aikido.Dto.Clubs;
using Aikido.Dto.Groups;
using Aikido.Dto.Users;
using Aikido.Exceptions;
using Aikido.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClubController : ControllerBase
    {
        private readonly ClubApplicationService _clubApplicationService;

        public ClubController(ClubApplicationService clubApplicationService)
        {
            _clubApplicationService = clubApplicationService;
        }


        /// <summary>
        /// Получение информации о конкретном клубе
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("get/{id}")]
        public async Task<ActionResult<ClubDto>> GetClubById(long id)
        {
            try
            {
                var club = await _clubApplicationService.GetClubByIdAsync(id);
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

        /// <summary>
        /// Поучение детальной информации о конкретном клубе
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("get/details/{id}")]
        public async Task<ActionResult<ClubDetailsDto>> GetClubDetails(long id)
        {
            try
            {
                var clubDetails = await _clubApplicationService.GetClubDetailsAsync(id);
                return Ok(clubDetails);
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

        /// <summary>
        /// Получение всех активных клубов
        /// </summary>
        /// <returns></returns>
        [HttpGet("get/all")]
        public async Task<ActionResult<List<ClubDto>>> GetAllClubs()
        {
            try
            {
                var clubs = await _clubApplicationService.GetAllClubsAsync();
                return Ok(clubs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении списка клубов", Details = ex.Message });
            }
        }

        /// <summary>
        /// Получение персонала клуба
        /// </summary>
        /// <param name="clubId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("get/{clubId}/staff")]
        public async Task<ActionResult<List<ClubStaffDto>>> GetClubStaff(long clubId)
        {
            try
            {
                var staff = await _clubApplicationService.GetClubStaff(clubId);
                return Ok(staff);
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { Message = "Сущность не найдена", Details = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(422, new { Message = "Операция невозможна", Details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении участников клуба", Details = ex.Message });
            }
        }

        /// <summary>
        /// Изменение состава персонала клуба
        /// </summary>
        /// <param name="clubId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("update/{clubId}/staff")]
        public async Task<IActionResult> UpdateClubStaff(long clubId, [FromBody] List<long> userIds)
        {
            try
            {
                await _clubApplicationService.UpdateClubStaff(clubId, userIds);
                return NoContent();
            }
            catch(EntityNotFoundException ex)
            {
                return NotFound(new { Message = "Сущность не найдена", Details = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(422, new { Message = "Операция невозможна", Details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении участников клуба", Details = ex.Message });
            }
        }
        /// <summary>
        /// Получение активных участников клуба
        /// </summary>
        /// <param name="clubId"></param>
        /// <returns></returns>
        [HttpGet("get/{clubId}/members")]
        public async Task<ActionResult<List<UserShortDto>>> GetClubMembers(long clubId)
        {
            try
            {
                var members = await _clubApplicationService.GetClubMembersAsync(clubId);
                return Ok(members);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении участников клуба", Details = ex.Message });
            }
        }

        /// <summary>
        /// Получение активных групп клуба
        /// </summary>
        /// <param name="clubId"></param>
        /// <returns></returns>
        [HttpGet("get/{clubId}/groups")]
        public async Task<ActionResult<List<GroupDto>>> GetClubGroups(long clubId)
        {
            try
            {
                var groups = await _clubApplicationService.GetClubGroups(clubId);
                return Ok(groups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении групп клуба", Details = ex.Message });
            }
        }

        /// <summary>
        /// Получение всех активных клубов руководителя
        /// </summary>
        /// <param name="managerId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("get/{managerId}/clubs")]
        public async Task<ActionResult<List<ClubDto>>> GetManagerClubs(long managerId)
        {
            try
            {
                var clubs = await _clubApplicationService.GetManagerClubsAsync(managerId);
                return Ok(clubs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении клубов руководителя", Details = ex.Message });
            }
        }

        /// <summary>
        /// Получение активных групп указанных клубов
        /// </summary>
        /// <param name="clubIds"></param>
        /// <returns></returns>
        [HttpGet("get/clubs/groups")]
        public async Task<ActionResult<List<GroupShortDto>>> GetClubsAllGroups([FromQuery] List<long> clubIds)
        {
            try
            {
                var groups = new List<GroupShortDto>();

                foreach(var clubId in clubIds)
                {
                    var clubGroups = await _clubApplicationService.GetClubGroups(clubId);

                    groups.AddRange(clubGroups.Select(g => new GroupShortDto(g)));
                }

                return Ok(groups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении групп клубов", Details = ex.Message });
            }
        }

        /// <summary>
        /// Создание клуба
        /// </summary>
        /// <param name="clubData"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateClub([FromBody] ClubCreationDto clubData)
        {
            try
            {
                var clubId = await _clubApplicationService.CreateClubAsync(clubData);
                return Ok(new { id = clubId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Обновление информации о клубе
        /// </summary>
        /// <param name="id"></param>
        /// <param name="clubData"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateClub(long id, [FromBody] ClubCreationDto clubData)
        {
            try
            {
                await _clubApplicationService.UpdateClubAsync(id, clubData);
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

        /// <summary>
        /// Обновление руководителя клуба(Админ)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="managerId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPatch("update/{id}/manager")]
        public async Task<IActionResult> UpdateClubManager(long id, [FromBody] long? managerId)
        {
            try
            {
                await _clubApplicationService.UpdateClubManager(id, managerId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }


        /// <summary>
        /// Мягкое удаление клуба
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("close/{id}")]
        public async Task<IActionResult> CloseClub(long id)
        {
            try
            {
                await _clubApplicationService.CloseClubAsync(id);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(409, new { Message = "Операция невозможна", Details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Восстановление клуба после мягкого восстановления
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPatch("recover/{id}")]
        public async Task<IActionResult> RecoverClub(long id)
        {
            try
            {
                await _clubApplicationService.RecoverClubAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// Полное удаление клуба
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteClub(long id)
        {
            try
            {
                await _clubApplicationService.DeleteClubAsync(id);
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