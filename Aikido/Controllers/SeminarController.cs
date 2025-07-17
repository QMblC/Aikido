using Aikido.Dto;
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

        public SeminarController(
            UserService userService,
            ClubService clubService,
            GroupService groupService,
            SeminarService seminarService)
        {
            this.userService = userService;
            this.clubService = clubService;
            this.groupService = groupService;
            this.seminarService = seminarService;
        }

        [HttpGet("get/{seminarId}")]
        public async Task<IActionResult> GetSeminar(long seminarId)
        {
            try
            {
                var seminar = await seminarService.GetSeminar(seminarId);

                return(Ok(new SeminarDto(seminar)));
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

    }
}
