﻿using Aikido.Dto;
using Aikido.Requests;
using Aikido.Services;
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

        public SeminarController(
            UserService userService,
            ClubService clubService,
            GroupService groupService,
            SeminarService seminarService,
            TableService tableService)
        {
            this.userService = userService;
            this.clubService = clubService;
            this.groupService = groupService;
            this.seminarService = seminarService;
            this.tableService = tableService;
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
                    var creator = await userService.GetUserById(seminarId);
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

        [HttpGet("member/get/{memberId}")]
        public async Task<IActionResult> GetSeminarMember(long memberId)
        {
            try
            {
                var member = await seminarService.GetSeminarMember(memberId);
                return Ok(new SeminarMemberDto(member));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("member/create")]
        public async Task<IActionResult> CreateSeminarMember([FromForm] SeminarMemberRequest request)
        {
            SeminarMemberDto memberDto;
            try
            {
                memberDto = await request.Parse();
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при разборе JSON участника: {ex.Message}");
            }

            try
            {
                var memberId = await seminarService.CreateSeminarMember(memberDto);
                return Ok(memberId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при создании участника: {ex.Message}");
            }
        }

        [HttpPut("member/update/{memberId}")]
        public async Task<IActionResult> UpdateSeminarMember(long memberId, [FromForm] SeminarMemberRequest request)
        {
            SeminarMemberDto memberDto;
            try
            {
                memberDto = await request.Parse();
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при разборе JSON участника: {ex.Message}");
            }

            try
            {
                await seminarService.UpdateSeminarMember(memberId, memberDto);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при обновлении участника: {ex.Message}");
            }
        }

        [HttpDelete("member/delete/{memberId}")]
        public async Task<IActionResult> DeleteSeminarMember(long memberId)
        {
            try
            {
                await seminarService.DeleteSeminarMember(memberId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при удалении участника: {ex.Message}");
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
                .Select(statement => new StatementDto(statement))
                .ToList();

            var seminar = await seminarService.GetSeminar(seminarId);
            var finalStatement = seminar.FinalStatementFile != null ? Convert.ToBase64String(seminar.FinalStatementFile) : null;

            var result = new
            {
                coachStatements,
                finalStatement
            };

            return Ok(result);
        }

        [HttpPost("statement/create")]
        public async Task<IActionResult> CreateCoachStatement(
            [FromQuery] long seminarId,
            [FromQuery] long coachId)
        {
            byte[] fileBytes;

            var coach = await userService.GetUserById(coachId);
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

            var clubs = await Task.WhenAll(
                coachStudents
                    .Select(u => u.ClubId)
                    .Where(id => id.HasValue)
                    .Distinct()
                    .Select(id => clubService.GetClubById(id.Value))
            );

            var groups = await Task.WhenAll(
                coachStudents
                    .Select(u => u.GroupId)
                    .Where(id => id.HasValue)
                    .Distinct()
                    .Select(id => groupService.GetGroupById(id.Value))
            );

            var tableStream = await tableService.CreateCoachStatement(coach, coachStudents, clubs.ToList(), groups.ToList(), seminar);

            fileBytes = tableStream.ToArray();

            var file = File(
                fileContents: tableStream.ToArray(),
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: $"{coach.FullName.Split(" ")[0]} ведомость " +
                $"семинара {seminar.Date.Day}.{seminar.Date.Month}.{seminar.Date.Year}.xlsx"
            );

            return file;
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
            [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не передан или пуст.");

            byte[] table;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                table = memoryStream.ToArray();
            }


            try
            {
                await seminarService.UpdateSeminarCoachStatement(seminarId, coachId, table);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
