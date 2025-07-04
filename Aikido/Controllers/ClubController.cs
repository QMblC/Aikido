using Aikido.Dto;
using Aikido.Requests;
using Aikido.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClubController : Controller
    {
        private readonly ClubService clubService;

        public ClubController(ClubService clubService)
        {
            this.clubService = clubService;
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

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
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
