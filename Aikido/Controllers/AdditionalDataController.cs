using Aikido.AdditionalData;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdditionalDataController : Controller
    {
        [HttpGet("get/enum/{enumName}")]
        public async Task<IActionResult> GetEnumNamesList(string enumName)
        {

            if (IsNameMatchEnum<Role>(enumName))
            {
                return Ok(EnumParser.GetEnumNames<Role>());
            }
            else if (IsNameMatchEnum<Sex>(enumName))
            {
                return Ok(EnumParser.GetEnumNames<Sex>());
            }
            else if (IsNameMatchEnum<Grade>(enumName))
            {
                return Ok(EnumParser.GetEnumNames<Grade>());
            }
            else if (IsNameMatchEnum<Education>(enumName))
            {
                return Ok(EnumParser.GetEnumNames<Education>());
            }
            else if (IsNameMatchEnum<ProgramType>(enumName))
            {
                return Ok(EnumParser.GetEnumNames<ProgramType>());
            }
            else
            {
                return BadRequest();
            }
        }

        private bool IsNameMatchEnum<T>(string enumName)
        {
            return enumName == typeof(T).Name || enumName == typeof(T).Name.ToLower();
        }
    }
}
