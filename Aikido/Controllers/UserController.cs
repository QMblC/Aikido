using Aikido.AdditionalData;
using Aikido.Application.Services;
using Aikido.Dto.Users;
using Aikido.Dto.Users.Creation;
using Aikido.Entities.Filters;
using Aikido.Requests;
using Aikido.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserApplicationService _userApplicationService;
        private readonly TableService _tableService;

        public UserController(
            UserApplicationService userApplicationService,
            TableService tableService)
        {
            _userApplicationService = userApplicationService;
            _tableService = tableService;
        }

        [HttpGet("get/{id}")]
        public async Task<ActionResult<UserDto>> GetUserDataById(long id)
        {
            try
            {
                var user = await _userApplicationService.GetUserByIdAsync(id);
                return Ok(user);
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

        [HttpGet("get/short-list")]
        public async Task<ActionResult<List<UserShortDto>>> GetUserShortList()
        {
            try
            {
                var users = await _userApplicationService.GetUserShortListAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ошибка при получении списка пользователей.");
            }
        }


        [HttpGet("find")]
        public async Task<ActionResult<UserShortDto>> FindUsers([FromQuery] UserFilter filter)
        {
            try
            {
                var shortUsers = await _userApplicationService.FindUsersAsync(filter);
                return Ok(shortUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("get-coach/{coachId}/students")]
        public async Task<ActionResult<UserShortDto>> FindCoachStudentsByName(long coachId, [FromQuery] string name)
        {
            try
            {
                var users = await _userApplicationService.GetCoachStudentsByName(coachId, name);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении клубов пользователя", Details = ex.Message });
            }
        }

        [HttpGet("get/short-list-cut-data/{startIndex}/{finishIndex}")]
        public async Task<ActionResult<UsersDataDto>> GetUserShortListCutData(
            int startIndex,
            int finishIndex,
            [FromQuery] UserFilter filter)
        {
            try
            {
                var result = await _userApplicationService.GetUserShortListCutDataAsync(startIndex, finishIndex, filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при получении списка пользователей. {ex.Message}");
            }
        }

        [HttpGet("get/{userId}/clubs")]
        public async Task<ActionResult<List<UserMembershipDto>>> GetUserMembership(long userId)//Fix 
        {
            try
            {
                var memberships = await _userApplicationService.GetUserMembershipsAsync(userId);
                return Ok(memberships
                    .Distinct()
                    .ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении клубов пользователя", Details = ex.Message });
            }
        }


        [HttpPost("{userId}/clubs/{clubId}/groups/{groupId}")]
        public async Task<IActionResult> AddUserMembership(long userId, long clubId, long groupId, [FromBody] string roleInGroup = "User")
        {
            try
            {
                await _userApplicationService.AddUserMembershipAsync(userId,
                    clubId,
                    groupId,
                    EnumParser.ConvertStringToEnum<Role>(roleInGroup));
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

        [HttpDelete("{userId}/groups/{groupId}")]
        public async Task<IActionResult> RemoveUserFromGroup(long userId, long groupId)
        {
            try
            {
                await _userApplicationService.RemoveUserMembershipAsync(userId, groupId);
                return Ok(new { Message = "Пользователь успешно удален из группы" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        [HttpGet("get/table")]
        public async Task<IActionResult> ExportUsers()
        {
            var stream = await _tableService.ExportUsersToExcelAsync();
            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "База пользователей.xlsx");
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] UserCreationDto userData)
        {
            try
            {
                var userId = await _userApplicationService.CreateUserAsync(userData);
                return Ok(new { id = userId });
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

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                await _userApplicationService.DeleteUserAsync(id);
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

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] UserCreationDto userData)
        {
            try
            {
                await _userApplicationService.UpdateUserAsync(id, userData);
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

        [HttpGet("get/table-template-for-update")]
        public async Task<IActionResult> GetUserUpdateTemplate()
        {
            try
            {
                var stream = await _tableService.GenerateUserUpdateTemplateExcelAsync();
                return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Шаблон пользователей.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при создании шаблона.", Details = ex.Message });
            }
        }

        [HttpGet("get/table-template-for-create")]
        public async Task<IActionResult> GetUserCreateTemplate()
        {
            try
            {
                var stream = await _tableService.GenerateUserCreateTemplateExcelAsync();
                return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Шаблон пользователей.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при создании шаблона.", Details = ex.Message });
            }
        }
    }
}