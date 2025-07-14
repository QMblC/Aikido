using Aikido.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeminarController : Controller
    {
        private readonly UserService userService;
    }
}
