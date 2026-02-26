using Aikido.Dto.Statistic;
using Aikido.Entities.Filters;
using Aikido.Services.ApplicationServices;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticController : ControllerBase
    {
        private readonly StatisticApplicationService _statisticApplicationService;

        public StatisticController(StatisticApplicationService statisticApplicationService)
        {
            _statisticApplicationService = statisticApplicationService;
        }

        /// <summary>
        /// Количество тренировок в указанном году
        /// </summary>
        /// <param name="year"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet("yearly-attendance/{year}")]
        public async Task<ActionResult<StatisticMetricDto>> GetYearlyAttendanceCount(int year, [FromQuery] StatAttendanceFilter filter)
        {
            var metric = await _statisticApplicationService.GetYearlyAttendances(year, filter);
            return Ok(metric);
        }

        /// <summary>
        /// Статистика посещений по годам
        /// </summary>
        /// <param name="firstYear"></param>
        /// <param name="lastYear"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet("yearly-attendance/{firstYear}/{lastYear}")]
        public async Task<ActionResult<List<StatisticMetricDto>>> GetYearlyAttendanceCount(
            int firstYear,
            int lastYear,
            [FromQuery] StatAttendanceFilter filter)
        {
            var metrics = await _statisticApplicationService.GetYearlyAttendances(firstYear, lastYear, filter);
            return Ok(metrics);
        }

        /// <summary>
        /// Процент посещений тренировок за год
        /// </summary>
        /// <param name="year"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet("yearly-attendance/{year}/percent")]
        public async Task<ActionResult<StatisticMetricDto>> GetYearlyAttendancePercent(int year, [FromQuery] StatAttendanceFilter filter)
        {
            try
            {
                var metric = await _statisticApplicationService.GetYearlyAttendancePercent(year, filter);
                return Ok(metric);
            }
            catch(ArgumentNullException ex)
            {
                return StatusCode(400, new { Message = "Значение объекта равно null или 0", Details = ex.Message });
            }
        }

        /// <summary>
        /// Статистика процента посещений по годам
        /// </summary>
        /// <param name="firstYear"></param>
        /// <param name="lastYear"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpGet("yearly-attendance/{firstYear}/{lastYear}/percent")]
        public async Task<ActionResult<StatisticMetricDto>> GetYearlyAttendancePercent(int firstYear,
            int lastYear,
            [FromQuery] StatAttendanceFilter filter)
        {
            throw new NotImplementedException();
        }
    }
}
