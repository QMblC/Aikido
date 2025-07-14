using Aikido.AdditionalData;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdditionalDataController : Controller
    {
        [HttpGet("get/enum/{enumName}")]
        public IActionResult GetEnumValuesWithEnumMember(string enumName)
        {
            var enumType = typeof(Role).Assembly
                .GetTypes()
                .FirstOrDefault(t => t.IsEnum &&
                    t.Namespace == "Aikido.AdditionalData" &&
                    string.Equals(t.Name, enumName, StringComparison.OrdinalIgnoreCase));

            if (enumType == null)
                return NotFound($"Enum с именем '{enumName}' не найден.");

            var values = enumType
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(f => f.GetCustomAttribute<System.Runtime.Serialization.EnumMemberAttribute>()?.Value)
                .Where(v => v != null)
                .ToList();

            return Ok(values);
        }

        [HttpGet("get/enums")]
        public IActionResult GetAllEnumValuesWithEnumMember()
        {
            var result = new Dictionary<string, List<string>>();

            var enumTypes = typeof(Role).Assembly
                .GetTypes()
                .Where(t => t.IsEnum && t.Namespace == "Aikido.AdditionalData");

            foreach (var enumType in enumTypes)
            {
                var values = enumType
                    .GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Select(f => f.GetCustomAttribute<System.Runtime.Serialization.EnumMemberAttribute>()?.Value)
                    .Where(v => v != null)
                    .ToList();

                result[enumType.Name] = values;
            }

            return Ok(result);
        }


    }
}
