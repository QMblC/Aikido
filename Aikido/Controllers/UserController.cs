using Aikido.Data;
using Aikido.Dto;
using Aikido.Requests;
using Aikido.Services;
using Microsoft.AspNetCore.Mvc;
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

        public UserController(UserService userService, ClubService clubService, GroupService groupService)
        {
            this.userService = userService;
            this.clubService = clubService;
            this.groupService = groupService;

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

        [HttpGet("get/short-list-cut-data")]
        public async Task<IActionResult> GetUserShortListCutData([FromForm] UserIndexesRequest request)
        {
            UserIndexesDto userIndexes;

            try
            {
                userIndexes = await request.Parse();
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при обработке JSON: {ex.Message}");
            }

            try
            {
                var users = await userService.GetUserListAlphabetAscending(userIndexes.StartIndex, userIndexes.FinishIndex);

                var usersResponse = new List<UserBaseResponseDto>();

                for (var i = 0; i < users.Count(); i++)
                {
                    var groupId = (long)users[i].GroupId;

                    var group = await groupService.GetGroupById(groupId);

                    var club = await clubService.GetClubById(group.ClubId);

                    var newResponse = new UserBaseResponseDto
                    {
                        Id = users[i].Id,
                        Role = users[i].Role.ToString(),
                        Login = users[i].Login,
                        FullName = users[i].FullName,
                        Photo = Convert.ToBase64String(users[i].Photo),
                        Birthday = users[i].Birthday,
                        City = users[i].City,
                        Grade = users[i].City,
                        ClubId = club.Id,
                        ClubName = club.Name
                    };

                    usersResponse.Add(newResponse);           
                }

                return Ok(usersResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ошибка при получении списка пользователей.");
            }
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

    }
}
