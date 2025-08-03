using Aikido.Dto;
using Aikido.Dto.Seminars;
using Aikido.Entities.Seminar;
using Aikido.Requests;
using Aikido.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeminarController : Controller
    {
        private readonly UserService userService;
        private readonly ClubService clubService;
        private readonly GroupService groupService;
        private readonly SeminarService seminarService;
        private readonly TableService tableService;
        private readonly PaymentService paymentService;

        public SeminarController(
            UserService userService,
            ClubService clubService,
            GroupService groupService,
            SeminarService seminarService,
            TableService tableService,
            PaymentService paymentService)
        {
            this.userService = userService;
            this.clubService = clubService;
            this.groupService = groupService;
            this.seminarService = seminarService;
            this.tableService = tableService;
            this.paymentService = paymentService;
        }

        [HttpGet("get/{seminarId}")]
        public async Task<IActionResult> GetSeminar(long seminarId)
        {
            try
            {
                var seminar = await seminarService.GetSeminar(seminarId);
                var seminarDto = new SeminarDto(seminar);

                if (seminarDto.CreatorId != null)
                {
                    var creator = await userService.GetByIdOrThrowException(seminarDto.CreatorId.Value);
                    seminarDto.Creator = new UserShortDto(creator);
                }       

                return Ok(seminarDto);
            }
            catch(KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("get/list")]
        public async Task<IActionResult> GetSeminarList()
        {
            var seminars = await seminarService.GetSeminarList();

            return Ok(seminars.Select(seminar => new SeminarDto(seminar)));
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSeminar([FromForm] SeminarRequest request)
        {
            SeminarDto seminarDto;
            try
            {
                seminarDto = await request.Parse();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + " " + request.SeminarDataJson);
            }

            try
            {
                var seminarId = await seminarService.CreateSeminar(seminarDto);
                return Ok(seminarId);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("delete/{seminarId}")]
        public async Task<IActionResult> DeleteSeminar(long seminarId)
        {
            try
            {
                await seminarService.DeleteSeminar(seminarId);
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPut("update/{seminarId}")]
        public async Task<IActionResult> UpdateSeminar(long seminarId, [FromForm] SeminarRequest request)
        {
            SeminarDto seminarDto;
            try
            {
                seminarDto = await request.Parse();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            try
            {
                await seminarService.UpdateSeminar(seminarId, seminarDto);
                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        [HttpGet("members/get/list-by-seminar/{seminarId}")]
        public async Task<IActionResult> GetSeminarMembersList(long seminarId)
        {
            return Ok(await seminarService.GetMembersBySeminarId(seminarId));
        }

        [HttpGet("get/statements/{seminarId}")]
        public async Task<IActionResult> GetSeminarStatements(long seminarId)
        {
            var coachStatements = seminarService
                .GetSeminarCoachStatements(seminarId)
                .Result
                .Select(statement => new StatementDto(statement))
                .ToList();

            var seminar = await seminarService.GetSeminar(seminarId);
            var finalStatement = seminar.FinalStatementFile != null ? Convert.ToBase64String(seminar.FinalStatementFile) : null;
            var isFinalStatementApplied = seminar.IsFinalStatementApplied;

            var result = new
            {
                coachStatements,
                finalStatement,
                isFinalStatementApplied
            };

            return Ok(result);
        }

        [HttpGet("statement/get")]
        public async Task<IActionResult> GetCoachStatement(
            [FromQuery] long seminarId,
            [FromQuery] long coachId)
        {
            byte[] fileBytes;

            var coach = await userService.GetByIdOrThrowException(coachId);
            var seminar = await seminarService.GetSeminar(seminarId);

            if (seminarService.Contains(seminarId, coachId))
            {
                var statement = await seminarService.GetCoachStatement(seminarId, coachId);

                fileBytes = statement.StatementFile.ToArray();

                return File(
                    fileContents: fileBytes,
                    contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileDownloadName: $"{coach.FullName.Split(" ")[0]} ведомость " +
                    $"семинара {seminar.Date.Day}.{seminar.Date.Month}.{seminar.Date.Year}.xlsx"
                );
            }

            var coachStudentIds = await groupService.GetCoachStudentsIds(coachId);
            var coachStudents = await userService.GetUsers(coachStudentIds);

            var members = coachStudents.Select(async student => new SeminarMemberDto(student,
                await clubService.GetClubById(student.ClubId.Value), seminar, coach))
                .Select(m => m.Result)
                .ToList();

            var tableStream = await tableService.CreateStatement(members, seminar);

            fileBytes = tableStream.ToArray();       

            var file = File(
                fileContents: tableStream.ToArray(),
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: $"{coach.FullName.Split(" ")[0]} ведомость " +
                $"семинара {seminar.Date.Day}.{seminar.Date.Month}.{seminar.Date.Year}.xlsx"
            );

            return file;
        }

        [HttpPost("statement/create")]
        public async Task<IActionResult> CreateCoachStatement(
            [FromQuery] long seminarId,
            [FromQuery] long coachId,
            [FromForm] TableRequest request)
        {
            var table = await request.Parse();

            var members = tableService.ParseStatement(table);
            var seminar = await seminarService.GetSeminar(seminarId);
            var name = $"{members.FirstOrDefault().CoachName} ведомость семинара {seminar.Date}";

            try
            {
                if (seminarService.Contains(seminarId, coachId))
                {
                    await seminarService.UpdateSeminarCoachStatement(seminarId, coachId, table, name);
                }
                else
                {
                    await seminarService.CreateSeminarCoachStatement(seminarId, coachId, table, name);
                }

                    
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            throw new NotImplementedException();
        }


        [HttpDelete("statement/delete")]
        public async Task<IActionResult> DeleteCoachStatement([FromQuery] long seminarId, [FromQuery] long coachId)
        {
            try
            {
                await seminarService.DeleteSeminarCoachStatement(seminarId, coachId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("statement/update")]
        public async Task<IActionResult> UpdateCoachStatement(
            [FromQuery] long seminarId,
            [FromQuery] long coachId,
            [FromForm] TableRequest request)
        {
            var table = await request.Parse(); 
            var members = tableService.ParseStatement(table);
            var seminar = await seminarService.GetSeminar(seminarId);

            var name = $"{members.FirstOrDefault().CoachName} ведомость семинара {seminar.Date}";

            try
            {
                await seminarService.UpdateSeminarCoachStatement(seminarId, coachId, table, name);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("statement/get/members")]
        public async Task<IActionResult> GetCoachStatementMembers(
            [FromQuery] long seminarId,
            [FromQuery] long coachId)
        {
            var coach = await userService.GetByIdOrThrowException(coachId);
            var seminar = await seminarService.GetSeminar(seminarId);

            var members = new List<SeminarMemberDto>();

            if (seminarService.Contains(seminarId, coachId))
            {
                var statement = await seminarService.GetCoachStatement(seminarId, coachId);
                members = tableService.ParseStatement(statement.StatementFile);
                return Ok(members);
            }

            var coachStudentIds = await groupService.GetCoachStudentsIds(coachId);
            var coachStudents = await userService.GetUsers(coachStudentIds);

            foreach (var student in coachStudents)
            {
                var club = await clubService.GetClubById(student.ClubId.Value);
                var group = await groupService.GetGroupById(student.GroupId.Value);

                var member = new SeminarMemberDto(student, club, seminar, coach);

                member.SeminarPrice = seminar.PriceSeminarInRubles;
                member.BudoPassportPrice = member.IsBudoPassportPayed ? 0 : seminar.PriceBudoPassportRubles;
                member.AnnualFee = member.IsAnnualFeePayed ? 0 : seminar.PriceAnnualFeeRubles;

                members.Add(member);

            }

            return Ok(members);

            throw new NotImplementedException();
        }

        [HttpPost("statement/create/members")]
        public async Task<IActionResult> CreateCoachStatementMembers(
            [FromQuery] long seminarId,
            [FromQuery] long coachId,
            CoachStatementMembersRequest request)
        {
            List<SeminarMemberDto> members;

            try
            {
                members = await request.Parse();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            try
            {
                var seminar = await seminarService.GetSeminar(seminarId);

                var table = await tableService.CreateCoachStatement(members, seminar);
                var name = $"{members.FirstOrDefault().CoachName} ведомость семинара {seminar.Date}";
                if (table == null)
                {
                    return StatusCode(500, "Не удалось создать таблицу");
                }

                if (seminarService.Contains(seminarId, coachId))
                {
                    await seminarService.UpdateSeminarCoachStatement(seminarId, coachId, table.ToArray(), name);
                }
                else
                {
                    await seminarService.CreateSeminarCoachStatement(seminarId, coachId, table.ToArray(), name);                    
                }

                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("get/final-statement/members/{seminarId}")]
        public async Task<IActionResult> GetFinalStatement(long seminarId)
        {
            SeminarEntity seminar;

            try
            {
                seminar = await seminarService.GetSeminar(seminarId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            var members = new List<SeminarMemberDto>();

            if (seminar.FinalStatementFile != null)
            {
                members = tableService.ParseStatement(seminar.FinalStatementFile);
                return Ok(members);
            }

            var statements = await seminarService.GetSeminarCoachStatements(seminarId);
            

            foreach (var statement in statements)
            {
                var currentMembers = tableService.ParseStatement(statement.StatementFile);
                members.AddRange(currentMembers);
            }

            members = members
                .Distinct()
                .ToList();

            return Ok(members);

            throw new NotImplementedException();
        }

        [HttpPost("create/final-statement/members/{seminarId}")]
        public async Task<IActionResult> CreateFinalStatement(long seminarId, CoachStatementMembersRequest request)
        {
            List<SeminarMemberDto> members;

            try
            {
                members = await request.Parse();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            try
            {
                var seminar = await seminarService.GetSeminar(seminarId);

                var table = await tableService.CreateCoachStatement(members, seminar);

                if (table == null)
                {
                    return StatusCode(500, "Не удалось создать таблицу");
                }

                if (seminar.FinalStatementFile != null)
                {
                    await seminarService.CreateFinalStatement(seminarId, table.ToArray());
                }
                else
                {
                    await seminarService.CreateFinalStatement(seminarId, table.ToArray());
                }                   

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("delete/final-statement/{seminarId}")]
        public async Task<IActionResult> DeleteFinalStatement(long seminarId)
        {
            try
            {
                var seminar = await seminarService.GetSeminar(seminarId);

                var oldMembers = tableService.ParseStatement(seminar.FinalStatementFile);
                await seminarService.DeleteFinalStatement(seminarId);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("get/final-statement/{seminarId}")]
        public async Task<IActionResult> GetFinalStatementTable(long seminarId)
        {
            SeminarEntity seminar;

            try
            {
                seminar = await seminarService.GetSeminar(seminarId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            try
            {
                if (seminar.FinalStatementFile != null)
                {
                    return File(
                        fileContents: seminar.FinalStatementFile,
                        contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileDownloadName: $"Итоговая ведомость " +
                        $"семинара {seminar.Date.Day}.{seminar.Date.Month}.{seminar.Date.Year}.xlsx");
                }

                var members = new List<SeminarMemberDto>();

                var statements = await seminarService.GetSeminarCoachStatements(seminarId);


                foreach (var statement in statements)
                {
                    var currentMembers = tableService.ParseStatement(statement.StatementFile);
                    members.AddRange(currentMembers);
                }

                members = members
                    .Distinct()
                    .ToList();

                var table = await tableService.CreateCoachStatement(members, seminar);

                return File(
                        fileContents: table.ToArray(),
                        contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileDownloadName: $"Итоговая ведомость " +
                        $"семинара {seminar.Date.Day}.{seminar.Date.Month}.{seminar.Date.Year}.xlsx");

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Не удалось отправить таблицу");
            }         
        }

        [HttpPost("create/final-statement/{seminarId}")]
        public async Task<IActionResult> CreateFinalStatementTable(long seminarId,
            [FromForm] TableRequest request)
        {

            var table = await request.Parse();
            var seminar = await seminarService.GetSeminar(seminarId);
            var members = tableService.ParseStatement(table);

            try
            {
                if (seminar.FinalStatementFile != null)
                {       
                    await seminarService.CreateFinalStatement(seminarId, table);

                }
                else
                {
                    await seminarService.CreateFinalStatement(seminarId, table);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("apply/final-statement/{seminarId}")]
        public async Task<IActionResult> ApplyChanges(long seminarId)
        {
            try
            {
                var seminar = await seminarService.GetSeminar(seminarId);

                var members = tableService.ParseStatement(seminar.FinalStatementFile);

                foreach (var member in members)
                {
                    await paymentService.CreatePayment(member, seminar);
                    await userService.ApplySeminarResults(member, seminar);
                }

                await seminarService.UpdateAppliement(seminarId, true);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("discard/final-statement/{seminarId}")]
        public async Task<IActionResult> DiscardChanges(long seminarId)
        {
            try
            {
                var seminar = await seminarService.GetSeminar(seminarId);

                var members = tableService.ParseStatement(seminar.FinalStatementFile);

                foreach (var member in members)
                {
                    await paymentService.DeletePayment(member, seminar);
                    await userService.DiscardSeminarResult(member, seminar);
                }

                await seminarService.UpdateAppliement(seminarId, false);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
