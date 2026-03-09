using Aikido.Dto.Statistic;
using Aikido.Entities.Filters;
using Aikido.Services.DatabaseServices.StatisticService;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Aikido.Services.ApplicationServices
{
    public class StatisticApplicationService
    {
        private readonly IStatisticDbService _statisticService;

        #region Attendance

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
                return 0;
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

        public async Task<List<StatisticMetricDto>> GetMonthlyAttendance(int year, StatAttendanceFilter filter)
        {
            var attendances = await _statisticService.GetYearlyAttendancesByMonthes(year, filter);

            return GetStatByMonthes(year, attendances);
        }

        #endregion

        #region PupilGrow

        public async Task<StatisticMetricDto> GetPupilGrowPercent(int year, StatAttendanceFilter filter)
        {
            var currentYear = await _statisticService.GetMonthlyPupilAmount(year, filter);
            

            if (currentYear[new DateTime(year - 1, 12, 1, 0, 0, 0, DateTimeKind.Utc)] != 0)
            {
                var diff = currentYear[new DateTime(year, 12, 1, 0, 0, 0, DateTimeKind.Utc)]
                    - currentYear[new DateTime(year - 1, 12, 1, 0, 0, 0, DateTimeKind.Utc)] * 1.0;
                return new StatisticMetricDto(
                    Math.Round((diff / currentYear[new DateTime(year - 1, 12, 1, 0, 0, 0, DateTimeKind.Utc)]) * 100, 2),
                    diff);
            }
            else
            {
                var diff = currentYear[new DateTime(year, 12, 1, 0, 0, 0, DateTimeKind.Utc)];
                return new StatisticMetricDto(null, diff);
            }
        }

        public async Task<List<StatisticMetricDto>> GetMonthlyPupilAmount(int year, StatAttendanceFilter filter)
        {
            var pupilAmount = await _statisticService.GetMonthlyPupilAmount(year, filter);
            
            return GetStatByMonthes(year, pupilAmount);
        }

        #endregion

        #region PupilRetention

        public async Task<StatisticMetricDto> GetPupilRetention(int year, StatAttendanceFilter filter)
        {
            var currentRetention = await CalculateYearRetention(year, filter);
            var previousRetention = await CalculateYearRetention(year - 1, filter);


            double? diff = null;

            if (previousRetention != null)
                diff = Math.Round(currentRetention.Value - previousRetention.Value, 2);

            return new StatisticMetricDto(
                Math.Round(currentRetention.Value, 2),
                diff);
        }

        public async Task<List<StatisticMetricDto>> GetMonthlyPupilRetention(int year, StatAttendanceFilter filter)
        {
            var pupilAmount = await _statisticService.GetMonthlyPupilAmount(year, filter);
            var pupilLeft = await _statisticService.GetPupilLeft(year, filter);

            var metrics = new List<StatisticMetricDto>();

            var endMonth = DateTime.UtcNow.Year == year ? DateTime.UtcNow.Month : 12;

            double? prevRetention = null;

            for (var month = 1; month <= endMonth; month++)
            {
                var date = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);

                var startAmount = pupilAmount.GetValueOrDefault(date);

                double? retention = null;

                if (startAmount > 0)
                {
                    var left = pupilLeft.GetValueOrDefault(date);
                    retention = Math.Round(((startAmount - left) / (startAmount * 1.0)) * 100, 2);
                }

                double? diff = null;

                if (retention != null && prevRetention != null)
                {
                    diff = Math.Round(retention.Value - prevRetention.Value, 2);
                }

                metrics.Add(new StatisticMetricDto(
                    retention,
                    diff,
                    date.ToString()
                ));

                prevRetention = retention;
            }

            return metrics;
        }

        private async Task<double?> CalculateYearRetention(int year, StatAttendanceFilter filter)
        {
            var pupilAmount = await _statisticService.GetMonthlyPupilAmount(year, filter);
            var pupilLeft = await _statisticService.GetPupilLeft(year, filter);

            var months = pupilAmount.Keys.OrderBy(x => x).ToList();

            var retentions = new List<double>();

            foreach (var month in months)
            {
                var startAmount = pupilAmount.GetValueOrDefault(month);

                if (startAmount == 0)
                    continue;

                var left = pupilLeft.GetValueOrDefault(month);

                var retention = ((startAmount - left) / (startAmount * 1.0)) * 100;

                retentions.Add(retention);
            }

            if (!retentions.Any())
                return null;

            return retentions.Average();
        }

        #endregion

        #region Dan

        public async Task<StatisticMetricDto> GetDanGrowPercent(int year, StatAttendanceFilter filter)
        {
            var danAmount = await _statisticService.GetMonthlyDanAmount(year, filter);

            var lastYearEnding = new DateTime(year - 1, 12, 1, 0, 0, 0, DateTimeKind.Utc);
            var thisYearEnding = new DateTime(year, 12, 1, 0, 0, 0, DateTimeKind.Utc);

            if (DateTime.UtcNow.Year == year)
            {
                thisYearEnding = new DateTime(year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            }

            var lastYearCount = danAmount.GetValueOrDefault(lastYearEnding, 0);
            var thisYearCount = danAmount.GetValueOrDefault(thisYearEnding, 0);

            if (lastYearCount != 0)
            {
                var diff = thisYearCount - lastYearCount;
                var percent = Math.Round((diff * 100.0) / lastYearCount, 2);

                return new StatisticMetricDto(percent, diff);
            }
            else
            {
                var diff = thisYearCount;
                return new StatisticMetricDto(null, diff);
            }
        }

        public async Task<List<StatisticMetricDto>> GetMonthlyDanAmount(int year, StatAttendanceFilter filter)
        {
            var danAmount = await _statisticService.GetMonthlyDanAmount(year, filter);
            return GetStatByMonthes(year, danAmount);
        }

        #endregion

        private List<StatisticMetricDto> GetStatByMonthes(int year, Dictionary<DateTime, int> stat)
        {
            var metrics = new List<StatisticMetricDto>();

            var endMonth = DateTime.UtcNow.Year == year ? DateTime.UtcNow.Month : 12;

            metrics.Add(new(
                stat[new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc)],
                stat[new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc)]
                - stat[new DateTime(year - 1, 12, 1, 0, 0, 0, DateTimeKind.Utc)],
                new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToString()));

            if (endMonth < 2)
            {
                return metrics;
            }

            for (var month = 2; month <= endMonth; month++)
            {
                metrics.Add(new(
                    stat[new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc)],
                    stat[new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc)]
                    - stat[new DateTime(year, month - 1, 1, 0, 0, 0, DateTimeKind.Utc)],
                    new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc).ToString()));
            }

            return metrics;
        }
    }
}
