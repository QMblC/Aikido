using Aikido.Dto;
using Aikido.Requests;
using Aikido.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupController : Controller
    {
        private readonly GroupService groupService;

        public GroupController(GroupService groupService)
        {
            this.groupService = groupService;
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetGroupDataById(long id)
        {
            try
            {
                var group = await groupService.GetGroupById(id);
                return Ok(group);
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

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] GroupRequest request)
        {
            GroupDto groupData;

            try
            {
                groupData = await request.Parse();
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при обработке JSON: {ex.Message}");
            }

            long groupId;

            try
            {
                groupId = await groupService.CreateGroup(groupData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }

            return Ok(new { id = groupId });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                await groupService.DeleteGroup(id);
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


    }
}
