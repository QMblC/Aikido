using Aikido.AdditionalData;
using DocumentFormat.OpenXml.Features;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdditionalDataController : ControllerBase
    {
        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            try
            {
                var roles = Enum.GetValues<Role>()
                    .Select(r => new { Value = (int)r, Name = r.ToString() })
                    .ToList();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении ролей", Details = ex.Message });
            }
        }

        [HttpGet("grades")]
        public IActionResult GetGrades()
        {
            try
            {
                var grades = Enum.GetValues<Grade>()
                    .Select(g => new { Value = (int)g, Name = g.ToString() })
                    .ToList();
                return Ok(grades);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении разрядов", Details = ex.Message });
            }
        }

        [HttpGet("sex")]
        public IActionResult GetSex()
        {
            try
            {
                var sexes = Enum.GetValues<Sex>()
                    .Select(s => new { Value = (int)s, Name = s.ToString() })
                    .ToList();
                return Ok(sexes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении пола", Details = ex.Message });
            }
        }

        [HttpGet("education")]
        public IActionResult GetEducation()
        {
            try
            {
                var educations = Enum.GetValues<Education>()
                    .Select(e => new { Value = (int)e, Name = e.ToString() })
                    .ToList();
                return Ok(educations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении образования", Details = ex.Message });
            }
        }

        [HttpGet("program-types")]
        public IActionResult GetProgramTypes()
        {
            try
            {
                var programTypes = Enum.GetValues<ProgramType>()
                    .Select(p => new { Value = (int)p, Name = p.ToString() })
                    .ToList();
                return Ok(programTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении типов программ", Details = ex.Message });
            }
        }

        [HttpGet("age-groups")]
        public IActionResult GetAgeGroups()
        {
            try
            {
                var ageGroups = Enum.GetValues<AgeGroup>()
                    .Select(a => new { Value = (int)a, Name = a.ToString() })
                    .ToList();
                return Ok(ageGroups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении возрастных групп", Details = ex.Message });
            }
        }

        [HttpGet("payment-types")]
        public IActionResult GetPaymentTypes()
        {
            try
            {
                var paymentTypes = Enum.GetValues<PaymentType>()
                    .Select(p => new { Value = (int)p, Name = p.ToString() })
                    .ToList();
                return Ok(paymentTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении типов платежей", Details = ex.Message });
            }
        }

        [HttpGet("event-types")]
        public IActionResult GetEventTypes()
        {
            try
            {
                var eventTypes = Enum.GetValues<EventType>()
                    .Select(e => new { Value = (int)e, Name = e.ToString() })
                    .ToList();
                return Ok(eventTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении типов событий", Details = ex.Message });
            }
        }

        [HttpGet("seminar-member-statuses")]
        public IActionResult GetSeminarMemberStatuses()
        {
            try
            {
                var statuses = Enum.GetValues<SeminarMemberStatus>()
                    .Select(s => new { Value = (int)s, Name = s.ToString() })
                    .ToList();
                return Ok(statuses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении статусов участников семинара", Details = ex.Message });
            }
        }

        [HttpGet("exclusion-date-types")]
        public IActionResult GetExclusionDateTypes()
        {
            try
            {
                var types = Enum.GetValues<ExclusiveDateType>()
                    .Select(t => new { Value = (int)t, Name = t.ToString() })
                    .ToList();
                return Ok(types);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при получении типов исключающих дат", Details = ex.Message });
            }
        }

        [HttpGet("all")]
        public IActionResult GetAllEnums()
        {
            try
            {
                var result = new
                {
                    Roles = Enum.GetValues<Role>().Select(r => new { Value = (int)r, Name = r.ToString() }),
                    Grades = Enum.GetValues<Grade>().Select(g => new { Value = (int)g, Name = g.ToString() }),
                    Sex = Enum.GetValues<Sex>().Select(s => new { Value = (int)s, Name = s.ToString() }),
                    Education = Enum.GetValues<Education>().Select(e => new { Value = (int)e, Name = e.ToString() }),
                    ProgramTypes = Enum.GetValues<ProgramType>().Select(p => new { Value = (int)p, Name = p.ToString() }),
                    AgeGroups = Enum.GetValues<AgeGroup>().Select(a => new { Value = (int)a, Name = a.ToString() }),
                    PaymentTypes = Enum.GetValues<PaymentType>().Select(p => new { Value = (int)p, Name = p.ToString() }),
                    EventTypes = Enum.GetValues<EventType>().Select(e => new { Value = (int)e, Name = e.ToString() }),
                    SeminarMemberStatuses = Enum.GetValues<SeminarMemberStatus>().Select(s => new { Value = (int)s, Name = s.ToString() }),
                    ExclusionDateTypes = Enum.GetValues<ExclusiveDateType>().Select(t => new { Value = (int)t, Name = t.ToString() })
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