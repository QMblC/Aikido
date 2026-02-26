using Aikido.Dto.Statistic;
using Aikido.Entities.Filters;
using Aikido.Services.DatabaseServices.StatisticService;
using System.Threading.Tasks;

namespace Aikido.Services.ApplicationServices
{
    public class StatisticApplicationService
    {
        private readonly IStatisticDbService _statisticService;

        public StatisticApplicationService(IStatisticDbService statisticService)
        {
            _statisticService = statisticService;
        }

        public async Task<StatisticMetricDto> GetYearlyAttendances(int year, StatAttendanceFilter filter)
        {
            var currentYearStat = await _statisticService.GetYearlyAttendances(year, filter);
            var previousYearStat = await _statisticService.GetYearlyAttendances(year - 1, filter);

            return new StatisticMetricDto(currentYearStat, currentYearStat - previousYearStat);
        }

        public async Task<StatisticMetricDto> GetYearlyAttendancePercent(int year, StatAttendanceFilter filter)
        {
            var currentPercent = await CalculateAttendancePercent(year, filter);
            var previousPercent = await CalculateAttendancePercent(year - 1, filter);

            return new StatisticMetricDto(currentPercent, Math.Round(currentPercent - previousPercent, 2));
        }

        private async Task<double> CalculateAttendancePercent(int year, StatAttendanceFilter filter)
        {
            var attendances = await _statisticService.GetYearlyAttendances(year, filter);
            var trainings = await _statisticService.GetYearlyTrainings(year, filter);

            if (trainings == 0)
            {
                throw new ArgumentNullException("В году нет тренировок");
            }

            double currentPercent = (double)attendances * 100 / trainings;

            return Math.Round(currentPercent, 2);
        } 

        public async Task<List<StatisticMetricDto>> GetYearlyAttendances(int firstYear, int lastYear, StatAttendanceFilter filter)
        {
            var metrics = new List<StatisticMetricDto>();

            var attendances = await _statisticService.GetYearlyAttendances(firstYear - 1, lastYear, filter);

            for (var year = firstYear; year <= lastYear; year++)
            {
                metrics.Add(new StatisticMetricDto(attendances[year],
                    attendances[year] - attendances[year - 1],
                    year.ToString()));
            }

            return metrics;
        }

        public async Task<StatisticMetricDto> GetYearlyAttendancePercent(int firstYear, int lastYear, StatAttendanceFilter filter)
        {
            var metrics = new List<StatisticMetricDto>();

            

            throw new NotImplementedException();
        }
    }
}
