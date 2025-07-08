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

                var usersResponse = ParseUserToBaseResponse(pagedResult.Users);

                return Ok(new
                {
                    TotalCount = pagedResult.TotalCount,
                    Users = usersResponse
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ошибка при получении списка пользователей.");
            }
        }

        private async Task<List<UserBaseResponseDto>> ParseUserToBaseResponse(List<UserEntity> users)
        {
            var usersResponse = new List<UserBaseResponseDto>();

            foreach (var user in users)
            {
                var newResponse = new UserBaseResponseDto
                {
                    Id = user.Id,
                    Role = user.Role.ToString(),
                    Login = user.Login,
                    FullName = user.FullName,
                    Photo = Convert.ToBase64String(user.Photo),
                    Birthday = user.Birthday,
                    City = user.City,
                    Grade = user.Grade,
                    ClubId = null,
                    ClubName = null
                };

                if (user.ClubId != 0 && user.ClubId != null)
                {
                    var club = await clubService.GetClubById((long)user.ClubId);

                    newResponse.ClubId = user.ClubId;
                    newResponse.ClubName = club.Name;
                }

                usersResponse.Add(newResponse);
            }

            return usersResponse;
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
                userId = await userService.CreateUser(userData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }

            return Ok(new { id =  userId}); //ToDo Тут возможно нужно возвращать JWT
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
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
        public async Task<IActionResult> CreateUsersList([FromBody] UserListRequest request)
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
                var createdIds = await userService.CreateUsers(users);
                return Ok(new { CreatedUserIds = createdIds });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при создании пользователей.", Details = ex.Message });
            }
        }



        [HttpPost("create/from-table")]
        public async Task<IActionResult> ImportUsersFromExcel([FromForm] TableRequest request)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest("Файл не был предоставлен или он пустой.");
            }

            try
            {
                using var stream = request.File.OpenReadStream();
                await tableService.ImportUsersFromExcelAsync(stream);
                return Ok(new { Message = "Импорт пользователей выполнен успешно." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при импорте пользователей.", Details = ex.Message });
            }
        }

        [HttpPut("update/from-table")]
        public async Task<IActionResult> UpdateUsersFromExcel([FromForm] TableRequest request)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest("Файл не был предоставлен или он пустой.");
            }

            try
            {
                using var stream = request.File.OpenReadStream();
                await tableService.UpdateUsersFromExcelAsync(stream);
                return Ok(new { Message = "Обновление пользователей выполнено успешно." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при обновлении пользователей.", Details = ex.Message });
            }
        }

        [HttpGet("get/table-template")]
        public async Task<IActionResult> GetUserTemplate()
        {
            try
            {
                var stream = await tableService.GenerateUserTemplateExcelAsync();

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
