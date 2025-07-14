using Aikido.AdditionalData;
using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using Aikido.Entities.Filters;
using Aikido.Requests;
using Aikido.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService userService;
        private readonly ClubService clubService;
        private readonly GroupService groupService;
        private readonly TableService tableService;

        public UserController(
            UserService userService,
            ClubService clubService,
            GroupService groupService,
            TableService tableService)
        {
            this.userService = userService;
            this.clubService = clubService;
            this.groupService = groupService;
            this.tableService = tableService;
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetUserDataById(long id)
        {
            try
            {
                var user = await userService.GetUserById(id);
                
                if (user.ClubId == null && user.GroupId == null)
                {
                    var club = await clubService.GetClubById(user.ClubId.Value);
                    return Ok(new UserDto(user));
                }
                if (user.ClubId != null && user.GroupId == null)
                {
                    var club = await clubService.GetClubById(user.ClubId.Value);
                    return Ok(new UserDto(user, club));
                }
                if (user.ClubId != null && user.GroupId != null)
                {
                    var club = await clubService.GetClubById(user.ClubId.Value);
                    var group = await groupService.GetGroupById(user.GroupId.Value);

                    return Ok(new UserDto(user, club, group));
                }
                return BadRequest();

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
        public async Task<IActionResult> GetUserShortList()
        {
            try
            {
                var users = await userService.GetUserIdAndNamesAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ошибка при получении списка пользователей.");
            }
        }

        [HttpGet("get/short-list-cut-data/{startIndex}/{finishIndex}")]
        public async Task<IActionResult> GetUserShortListCutData(
            int startIndex,
            int finishIndex,
            [FromQuery] UserFilter filter)
        {
            try
            {
                var pagedResult = await userService.GetUserListAlphabetAscending(startIndex, finishIndex, filter);
                var users = pagedResult.Users;

                // Последовательно добавляем названия клубов и групп
                foreach (var user in users)
                {
                    if (user.ClubId != null)
                    {
                        var club = await clubService.GetClubById(user.ClubId.Value);
                        user.AddClubName(club);
                    }

                    if (user.GroupId != null)
                    {
                        var group = await groupService.GetGroupById(user.GroupId.Value);
                        user.AddGroupName(group);
                    }
                }

                return Ok(new
                {
                    TotalCount = pagedResult.TotalCount,
                    Users = users
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при получении списка пользователей. {ex.Message}");
            }
        }


        [HttpGet("get/table")]
        public async Task<IActionResult> ExportUsers()
        {
            var stream = await tableService.ExportUsersToExcelAsync();

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "База пользователей.xlsx");
        }


        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] UserRequest request)
        {
            UserDto userData;

            try
            {
                userData = await request.Parse();
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при обработке JSON: {ex.Message}");
            }

            long userId;

            try
            {
                if (userData.ClubId != null && !await clubService.Contains(userData.ClubId.Value))
                {
                    return BadRequest($"Клуба с Id = {userData.ClubId} не существует");
                }

                if (userData.GroupId != null && !await clubService.Contains(userData.GroupId.Value))
                {
                    return BadRequest($"Группы с Id = {userData.GroupId} не существует");
                }

                userId = await userService.CreateUser(userData);

                if (userData.GroupId != null)
                {
                    await groupService.AddUserToGroup((long)userData.GroupId, userId);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }

            return Ok(new { id =  userId});
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var userGroups = await groupService.GetGroupsByUser(id);

                foreach (var group in userGroups)
                {
                    await groupService.DeleteUserFromGroup(group, id);
                }

                await userService.DeleteUser(id);
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
        public async Task<IActionResult> Update(long id, [FromForm] UserRequest request)
        {
            UserDto userData;

            try
            {
                userData = await request.Parse();
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при обработке JSON: {ex.Message}");
            }

            try
            {
                if (userData.ClubId != null && !await clubService.Contains(userData.ClubId.Value))
                {
                    return BadRequest($"Клуба с Id = {userData.ClubId} не существует");
                }

                if (userData.GroupId != null && !await clubService.Contains(userData.GroupId.Value))
                {
                    return BadRequest($"Группы с Id = {userData.GroupId} не существует");
                }

                await userService.UpdateUser(id, userData);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }

            return Ok();
        }

        [HttpPost("create/list")]
        public async Task<IActionResult> CreateUsersList([FromForm] UserListRequest request)
        {
            List<UserDto> users;

            try
            {
                users = await request.Parse();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Ошибка обработки JSON.", Details = ex.Message });
            }

            if (users == null || !users.Any())
                return BadRequest("Список пользователей пустой.");

            try
            {
                foreach (var club in users.Select(u => u.ClubId))
                {
                    await clubService.GetClubById((long)club);
                }
            }
            catch
            {
                return BadRequest("В данных содержатся несуществующие Id клубов");
            }

            try
            {
                foreach (var group in users.Select(u => u.GroupId))
                {
                    await clubService.GetClubById((long)group);
                }
            }
            catch
            {
                return BadRequest("В данных содержатся несуществующие Id групп");
            }


            try
            {
                var createdIds = await userService.CreateUsers(users);
                
                return Ok(new { CreatedUserIds = createdIds });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при создании пользователей.", Details = ex.Message });
            }
        }

        [HttpPost("update/list")]
        public async Task<IActionResult> UpdateUsersList([FromForm] UserListRequest request)
        {
            List<UserDto> users;

            try
            {
                users = await request.Parse();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Ошибка обработки JSON.", Details = ex.Message });
            }

            if (users == null || !users.Any())
                return BadRequest("Список пользователей пустой.");

            try
            {
                await userService.UpdateUsers(users);
                return Ok(new { Message = "Пользователи успешно обновлены." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при обновлении пользователей.", Details = ex.Message });
            }
        }

        [HttpGet("get/table-template-for-update")]
        public async Task<IActionResult> GetUserUpdateTemplate()
        {
            try
            {
                var stream = await tableService.GenerateUserUpdateTemplateExcelAsync();

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
                var stream = await tableService.GenerateUserCreateTemplateExcelAsync();

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
