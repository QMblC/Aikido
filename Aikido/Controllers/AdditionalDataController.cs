using Aikido.AdditionalData.Enums;
using Aikido.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using System.Runtime.Serialization;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdditionalDataController : ControllerBase
    {

        [HttpGet("all")]
        public ActionResult<List<EnumItemDto>> GetAllEnums()
        {
            try
            {
                var result = new 
                {
                    Roles = Enum.GetValues<Role>().Select(r =>
                        new EnumItemDto(
                            r.ToString(),
                            r.GetAttributeOfType<EnumMemberAttribute>()?.Value ?? r.ToString()
                        )
    ),
                    Grades = Enum.GetValues<Grade>().Select(g =>
                        new EnumItemDto(
                            (g).ToString(),
                            g.GetAttributeOfType<EnumMemberAttribute>()?.Value ?? g.ToString()
                        )
    ),
                    Sex = Enum.GetValues<Sex>().Select(s =>
                        new EnumItemDto(
                            (s).ToString(),
                            s.GetAttributeOfType<EnumMemberAttribute>()?.Value ?? s.ToString()
                        )
    ),
                    Education = Enum.GetValues<Education>().Select(e =>
                        new EnumItemDto(
                            (e).ToString(),
                            e.GetAttributeOfType<EnumMemberAttribute>()?.Value ?? e.ToString()
                        )
    ),
                    ProgramTypes = Enum.GetValues<ProgramType>().Select(p =>
                        new EnumItemDto(
                            (p).ToString(),
                            p.GetAttributeOfType<EnumMemberAttribute>()?.Value ?? p.ToString()
                        )
    ),
                    AgeGroups = Enum.GetValues<AgeGroup>().Select(a =>
                        new EnumItemDto(
                            (a).ToString(),
                            a.GetAttributeOfType<EnumMemberAttribute>()?.Value ?? a.ToString()
                        )
    ),
                    PaymentTypes = Enum.GetValues<PaymentType>().Select(p =>
                        new EnumItemDto(
                            (p).ToString(),
                            p.GetAttributeOfType<EnumMemberAttribute>()?.Value ?? p.ToString()
                        )
    ),
                    EventTypes = Enum.GetValues<EventType>().Select(e =>
                        new EnumItemDto(
                            (e).ToString(),
                            e.GetAttributeOfType<EnumMemberAttribute>()?.Value ?? e.ToString()
                        )
    ),
                    SeminarMemberStatuses = Enum.GetValues<SeminarMemberStatus>().Select(s =>
                        new EnumItemDto(
                            (s).ToString(),
                            s.GetAttributeOfType<EnumMemberAttribute>()?.Value ?? s.ToString()
                        )
    ),
                    ExclusionDateTypes = Enum.GetValues<ExclusiveDateType>().Select(t =>
                        new EnumItemDto(
                            (t).ToString(),
                            t.GetAttributeOfType<EnumMemberAttribute>()?.Value ?? t.ToString()
                        )
    )
                };

                return Ok(result);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении всех перечислений", Details = ex.Message });
            }
        }
    }
}