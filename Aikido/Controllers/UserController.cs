using Aikido.Data;
using Aikido.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService userService;

        public UserController(UserService userService)
        {
            this.userService = userService;
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetUserDataById(long id)
        {
            try
            {
                var user = await userService.GetUserDataById(id);
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

        [HttpGet("get-ids")]
        public IActionResult GetIds()
        {
            return Ok(new { ids = new List<long> { 1, 2, 3 } });
        }

        [HttpPost("create")]
        public IActionResult Create()
        {
            return Ok(new { message = "successful" });
        }

        [HttpDelete("delete/{id}")]
        public IActionResult Delete(long id)
        {
            return Ok();
        }

        [HttpPut("update/{id}")]
        public IActionResult Update(long id)
        {
            return Ok();
        }
    }
}
