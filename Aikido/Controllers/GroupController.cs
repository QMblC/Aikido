using Aikido.Application.Services;
using Aikido.Dto;
using Aikido.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupController : ControllerBase
    {
        private readonly GroupApplicationService _groupApplicationService;

        public GroupController(GroupApplicationService groupApplicationService)
        {
            _groupApplicationService = groupApplicationService;
        }

        [HttpGet("get/{id}")]
        public async Task<ActionResult<GroupDto>> GetGroupById(long id)
        {
            try
            {
                var group = await _groupApplicationService.GetGroupByIdAsync(id);
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

        //[HttpGet("get/info/{id}")]
        public async Task<IActionResult> GetGroupInfo(long id)
        {
            try
            {
                var groupInfo = await _groupApplicationService.GetGroupInfoAsync(id);
                return Ok(groupInfo);
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
        public async Task<ActionResult<List<GroupDto>>> GetAllGroups()
        {
            try
            {
                var groups = await _groupApplicationService.GetAllGroupsAsync();
                return Ok(groups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении списка групп", Details = ex.Message });
            }
        }

        [HttpGet("get/by-user/{userId}")]
        public async Task<ActionResult<List<GroupDto>>> GetGroupsByUser(long userId)
        {
            try
            {
                var groups = await _groupApplicationService.GetGroupsByUserAsync(userId);
                return Ok(groups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении групп пользователя", Details = ex.Message });
            }
        }

        [HttpGet("get/{groupId}/members")]
        public async Task<ActionResult<List<UserShortDto>>> GetGroupMembers(long groupId)
        {
            try
            {
                var members = await _groupApplicationService.GetGroupMembersAsync(groupId);
                return Ok(members);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении участников группы", Details = ex.Message });
            }
        }

        [HttpPost("{groupId}/members/{userId}")]
        public async Task<IActionResult> AddUserToGroup(long groupId, long userId)
        {
            try
            {
                await _groupApplicationService.AddUserToGroupAsync(groupId, userId);
                return Ok(new { Message = "Пользователь успешно добавлен в группу" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        [HttpDelete("{groupId}/members/{userId}")]
        public async Task<IActionResult> RemoveUserFromGroup(long groupId, long userId)
        {
            try
            {
                await _groupApplicationService.RemoveUserFromGroupAsync(groupId, userId);
                return Ok(new { Message = "Пользователь успешно удален из группы" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateGroup([FromBody] GroupDto groupData)
        {
            try
            {
                var groupId = await _groupApplicationService.CreateGroupAsync(groupData);
                return Ok(new { id = groupId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateGroup(long id, [FromBody] GroupDto groupData)
        {
            try
            {
                await _groupApplicationService.UpdateGroupAsync(id, groupData);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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
        public async Task<IActionResult> DeleteGroup(long id)
        {
            try
            {
                await _groupApplicationService.DeleteGroupAsync(id);
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