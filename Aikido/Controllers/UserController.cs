using Aikido.Data;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetUserDataById(long id, [FromServices] AppDbContext context)
        {
            return Ok(new
            {
                id = id
            });
        }

        [HttpGet("get-ids")]
        public async Task<IActionResult> GetIds([FromServices] AppDbContext context)
        {
            return Ok(new
            {
                ids =  new List<long> { 1, 2, 3}
            });
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromServices] AppDbContext context)
        {
            return Ok(new
            {
                message = "sucsessful"
            });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete([FromServices] AppDbContext context)
        {
            return Ok();
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(long id, [FromServices] AppDbContext context)
        {
            return Ok();
        }
    }
}
