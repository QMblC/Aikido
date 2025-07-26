using Aikido.AdditionalData;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Runtime.Serialization;

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
            var result = new Dictionary<string, Dictionary<string, string>>();

            var enumTypes = typeof(Role).Assembly
                .GetTypes()
                .Where(t => t.IsEnum && t.Namespace == "Aikido.AdditionalData");

            foreach (var enumType in enumTypes)
            {
                var dict = enumType
                    .GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(f => f.GetCustomAttribute<EnumMemberAttribute>() != null)
                    .ToDictionary(
                        f => f.Name,
                        f => f.GetCustomAttribute<EnumMemberAttribute>()!.Value!
                    );

                result[enumType.Name] = dict;
            }

            return Ok(result);
        }
    }
}
