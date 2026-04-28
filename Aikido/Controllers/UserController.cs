using Aikido.AdditionalData;
using Aikido.AdditionalData.Enums;
using Aikido.Application.Services;
using Aikido.Dto.Users;
using Aikido.Dto.Users.Creation;
using Aikido.Entities.Filters;
using Aikido.Exceptions;
using Aikido.Requests;
using Aikido.Services;
using Aikido.Services.ApplicationServices;
using Aikido.Services.UnitOfWork;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserApplicationService _userApplicationService;
        private readonly UserMembershipApplicationService _userMembershipApplicationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TableService _tableService;

        public UserController(
            UserApplicationService userApplicationService,
            TableService tableService,
            UserMembershipApplicationService userMembershipApplicationService,
            IUnitOfWork unitOfWork)
        {
            _userApplicationService = userApplicationService;
            _tableService = tableService;
            _userMembershipApplicationService = userMembershipApplicationService;
            _unitOfWork = unitOfWork;
        }

        [Authorize(Roles = "Admin,Manager,Coach,User")]
        [HttpGet("get/{id}")]
        public async Task<ActionResult<UserDto>> GetUserDataById(long id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

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

        /// <summary>
        /// Получение указанного списка пользователей
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager,Coach")]
        [HttpGet("get/users")]
        public async Task<ActionResult<List<UserShortDto>>> GetUsersShortList([FromQuery]List<long> ids)
        {
            try
            {
                var users = await _userApplicationService.GetUsers(ids);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ошибка при получении списка пользователей.");
            }
        }

        [HttpGet("get/short-list")]
        public async Task<ActionResult<List<UserShortDto>>> GetUserShortList()
        {
            try
            {
                var users = await _userApplicationService.GetActiveUserShortListAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ошибка при получении списка пользователей.");
            }
        }

        /// <summary>
        /// Получение заархивированных пользователей
        /// </summary>
        /// <returns></returns>
        [HttpGet("get/archived-users")]
        public async Task<ActionResult<List<UserShortDto>>> GetArchivedUsers()
        {
            try
            {
                var users = await _userApplicationService.GetArchivedUsersAsync();
                return Ok(users);
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении списка пользователей.", Details = ex.Message});
            }
        }


        [HttpGet("find")]
        public async Task<ActionResult<UserShortDto>> FindUsers([FromQuery] UserFilter filter)
        {
            try
            {
                var shortUsers = await _userApplicationService.FindActiveUsersAsync(filter);
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

        [Authorize(Roles = "Admin,Manager,Coach")]
        [HttpGet("get/short-list-cut-data/{startIndex}/{finishIndex}")]
        public async Task<ActionResult<UsersDataDto>> GetUserShortListCutData(
            int startIndex,
            int finishIndex,
            [FromQuery] UserFilter filter)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var requestedById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            try
            {
                UsersDataDto? result;
                if (role == EnumParser.ConvertEnumToString(Role.Admin))
                {
                    result = await _userApplicationService.GetActiveUserShortListCutDataAsync(startIndex, finishIndex, filter);
                }
                else
                {
                    result = await _userApplicationService.GetActiveUserShortListCutDataAsync(startIndex, finishIndex, filter, requestedById);
                }

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
                var memberships = await _userMembershipApplicationService.GetUserMembershipsAsync(userId);
                return Ok(memberships
                    .Distinct()
                    .ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении клубов пользователя", Details = ex.Message });
            }
        }

        /// <summary>
        /// Добавление в группу
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userMembership"></param>
        /// <returns></returns>
        [HttpPost("{userId}/add/membership")]
        public async Task<IActionResult> AddUserMembership(long userId, [FromBody] UserMembershipCreationDto userMembership)
        {
            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    await _userMembershipApplicationService.AddUserMembershipAsync(userId, userMembership);       
                });

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

        /// <summary>
        /// Открепление от группы
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpDelete("{userId}/groups/{groupId}")]
        public async Task<IActionResult> RemoveUserFromGroup(long userId, long groupId)
        {
            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    await _userMembershipApplicationService.CloseUserMembershipAsync(userId, groupId);
                });
                
                return Ok(new { Message = "Пользователь успешно удален из группы" });
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { Message = "Не удалось найти членство в группе", Details = ex.Message });
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

        /// <summary>
        /// Мягкое удаление пользователя
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPatch("close/{id}")]
        public async Task<IActionResult> CloseUserAsync(long id)
        {
            try
            {
                await _userApplicationService.CloseUserAsync(id);
                return NoContent();
            }
            catch(InvalidOperationException ex)
            {
                return StatusCode(409, new { Messsage = "Невозможная операция", Details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }
        
        /// <summary>
        /// Восстановление пользователя
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPatch("recover/{id}")]
        public async Task<IActionResult> RecoverUserAsync(long id)
        {
            try
            {
                await _userApplicationService.RecoverUserAsync(id);
                return NoContent();
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

        [HttpPost("create/table")]
        public async Task<IActionResult> SetFinalStatement([FromForm] TableFileRequest tableFile)
        {
            try
            {
                var file = tableFile.Table;

                if (file == null || file.Length == 0)
                    return BadRequest("Файл Excel не найден или пустой!");

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    var partialMembers = _tableService.ParseUserCreationTable(stream);
                    foreach (var member in partialMembers)
                    {
                        await _userApplicationService.CreateUserAsync(member);
                    }
                    return Ok();
                }   
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при создании пользователей.", Details = ex.Message });
            }
        }

        /// <summary>
        /// История семинаров пользователя
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager,Coach,User")]
        [HttpGet("get/{id}/seminar-history")]
        public async Task<ActionResult<List<UserSeminarHistoryItemDto>>> GetUserSeminarHistoryById(long id)
        {
            try
            {
                var history = await _userApplicationService.GetUserSeminarHistory(id);
                return Ok(history);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        /// <summary>
        /// История аттестаций пользователя
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager,Coach,User")]
        [HttpGet("get/{id}/certification-history")]
        public async Task<ActionResult<List<UserSeminarHistoryItemDto>>> GetUserCertificationHistoryById(long id)
        {
            try
            {
                var history = await _userApplicationService.GetUserCertificationHistory(id);
                return Ok(history);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (EntityNotFoundException ex)
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