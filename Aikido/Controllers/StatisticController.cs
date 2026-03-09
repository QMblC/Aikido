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
        /// Количество посещений по месяцам
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpGet("monthly-attendance/{year}")]
        public async Task<ActionResult<List<StatisticMetricDto>>> GetMonthlyAttendance(int year, [FromQuery] StatAttendanceFilter filter)
        {
            var metrics = await _statisticApplicationService.GetMonthlyAttendance(year, filter);
            return Ok(metrics);
        }

        /// <summary>
        /// Процент роста учеников
        /// </summary>
        /// <param name="year"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpGet("pupil-grow-percent/{year}")]
        public async Task<ActionResult<StatisticMetricDto>> GetPupilGrowPercent(int year, [FromQuery] StatAttendanceFilter filter)
        {
            var metric = await _statisticApplicationService.GetPupilGrowPercent(year, filter);
            return Ok(metric);
        }

        /// <summary>
        /// Количество учеников по месяцам
        /// </summary>
        /// <param name="year"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpGet("pupil-amount/monthly/{year}")]
        public async Task<ActionResult<List<StatisticMetricDto>>> GetMonthlyPupilAmount(int year, [FromQuery] StatAttendanceFilter filter)
        {
            var metrics = await _statisticApplicationService.GetMonthlyPupilAmount(year, filter);
            return Ok(metrics);
        }

        /// <summary>
        /// Процент удержания учеников за год
        /// </summary>
        /// <param name="year"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpGet("pupil-retention/{year}")]
        public async Task<ActionResult<StatisticMetricDto>> GetPupilRetention(int year, [FromQuery] StatAttendanceFilter filter)
        {
            var metric = await _statisticApplicationService.GetPupilRetention(year, filter);
            return Ok(metric);
        }

        /// <summary>
        /// Процент удержания учеников по месяцам
        /// </summary>
        /// <param name="year"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpGet("pupil-retention/monthly/{year}")]
        public async Task<ActionResult<List<StatisticMetricDto>>> GetMonthlyPupilRetention(int year, [FromQuery] StatAttendanceFilter filter)
        {
            var metrics = await _statisticApplicationService.GetMonthlyPupilRetention(year, filter);
            return Ok(metrics);
        }

        /// <summary>
        /// Процент рота мастеров
        /// </summary>
        /// <param name="year"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet("dan-grow-percent/{year}")]
        public async Task<ActionResult<StatisticMetricDto>> GetDanGrowPercent(int year, [FromQuery] StatAttendanceFilter filter)
        {
            var metric = await _statisticApplicationService.GetDanGrowPercent(year, filter);
            return Ok(metric);
        }

        /// <summary>
        /// Количество мастеров по месяца
        /// </summary>
        /// <param name="year"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet("dan-amount/monthly/{year}")]
        public async Task<ActionResult<StatisticMetricDto>> GetMonthlyDanAmount(int year, [FromQuery] StatAttendanceFilter filter)
        {
            var metric = await _statisticApplicationService.GetMonthlyDanAmount(year, filter);
            return Ok(metric);
        }

        [HttpGet("seminar-activity-percent/average")]
        public async Task<ActionResult<StatisticMetricDto>> GetAverageSeminarActivityPercent(List<long> seminarIds)
        {
            throw new NotImplementedException();
        }

        [HttpGet("seminar-activity-percent")]
        public async Task<ActionResult<List<StatisticMetricDto>>> GetSeminarActivityPercent(List<long> seminarIds)
        {
            throw new NotImplementedException();
        }

        [HttpGet("seminar-certification-percent/average")]
        public async Task<ActionResult<StatisticMetricDto>> GetAverageSeminarCertificationPercent(List<long> seminarIds)
        {
            throw new NotImplementedException();
        }

        [HttpGet("seminar-certification-percent")]
        public async Task<ActionResult<List<StatisticMetricDto>>> GetSeminarCertificationPercent(List<long> seminarIds)
        {
            throw new NotImplementedException();
        }

        [HttpGet("seminar-money-income/average")]
        public async Task<ActionResult<StatisticMetricDto>> GetAverageSeminarMoneyIncome(List<long> seminarIds)
        {
            throw new NotImplementedException();
        }

        [HttpGet("seminar-money-income")]
        public async Task<ActionResult<List<StatisticMetricDto>>> GetSeminarMoneyIncome(List<long> seminarIds)
        {
            throw new NotImplementedException();
        }
    }
}
