using Aikido.AdditionalData;
using Aikido.Application.Services;
using Aikido.Dto;
using Aikido.Requests;
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

        [HttpGet("get/{clubId}/staff")]
        public async Task<ActionResult<UserShortDto>> GetClubStaff(long clubId)
        {
            try
            {
                var members = await _clubApplicationService.GetClubMembersAsync(clubId, Role.Coach);
                return Ok(members);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении участников клуба", Details = ex.Message });
            }
        }

        [HttpGet("get/{clubId}/members")]
        public async Task<ActionResult<UserShortDto>> GetClubMembers(long clubId)
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

        [HttpPost("create")]
        public async Task<IActionResult> CreateClub([FromBody] ClubDto clubData)
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

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateClub(long id, [FromBody] ClubDto clubData)
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