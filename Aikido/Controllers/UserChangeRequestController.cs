using Aikido.AdditionalData;
using Aikido.Dto;
using Aikido.Entities;
using Aikido.Services.DatabaseServices;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserChangeRequestController : ControllerBase
    {
        private readonly UserChangeRequestDbService _requestService;

        public UserChangeRequestController(UserChangeRequestDbService requestService)
        {
            _requestService = requestService;
        }

        [HttpPost("request/create")]
        public async Task<IActionResult> RequestCreateUser(
            [FromQuery] long coachId,
            [FromBody] UserDto userData)
        {
            try
            {
                var json = JsonSerializer.Serialize(userData);
                var data = JsonSerializer.Deserialize<UserDto>(json);
            }
            catch(Exception ex)
            {
                return StatusCode(400, new { Message = "Ошибка в исходных данных userData", Details = ex.Message });
            }

            try
            {
                userData.Role = Role.User.ToString();
                var requestId = await _requestService.CreateUserRequest(coachId, userData);
                return Ok(new { RequestId = requestId, Message = "Заявка отправлена на рассмотрение" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка", Details = ex.Message });
            }
        }

        [HttpPost("request/update/{userId}")]
        public async Task<IActionResult> RequestUpdateUser(
            long userId,
            [FromQuery] long coachId,
            [FromBody] UserDto userData)
        {
            try
            {
                var json = JsonSerializer.Serialize(userData);
                var data = JsonSerializer.Deserialize<UserDto>(json);
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { Message = "Ошибка в исходных данных userData", Details = ex.Message });
            }

            try
            {
                var requestId = await _requestService.UpdateUserRequest(
                    coachId,
                    userId,
                    userData);
                return Ok(new { RequestId = requestId, Message = "Заявка отправлена на рассмотрение" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка", Details = ex.Message });
            }
        }

        [HttpPost("request/delete/{userId}")]
        public async Task<IActionResult> RequestDeleteUser(long userId,
            [FromQuery] long coachId)
        {
            try
            {
                var requestId = await _requestService.DeleteUserRequest(coachId, userId);
                return Ok(new { RequestId = requestId, Message = "Заявка отправлена на рассмотрение" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка", Details = ex.Message });
            }
        }

        [HttpGet("get/pending")]
        public async Task<ActionResult<List<UserChangePendingRequestDto>>> GetPendingRequests()
        {
            try
            {
                var requests = await _requestService.GetPendingRequests();
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка", Details = ex.Message });
            }
        }

        [HttpPost("{requestId}/approve")]
        public async Task<IActionResult> ApproveRequest(long requestId)
        {
            try
            {
                await _requestService.ApproveRequest(requestId);
                return Ok(new { Message = "Заявка одобрена и применена" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка", Details = ex.Message });
            }
        }

        [HttpPost("{requestId}/reject")]
        public async Task<IActionResult> RejectRequest(long requestId)
        {
            try
            {
                await _requestService.RejectRequest(requestId);
                return Ok(new { Message = "Заявка отклонена" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка", Details = ex.Message });
            }
        }
    }
}
