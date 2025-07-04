﻿using Aikido.Data;
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

    }
}
