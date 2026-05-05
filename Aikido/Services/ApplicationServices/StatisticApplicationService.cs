using Aikido.Dto.Statistic;
using Aikido.Entities.Filters;
using Aikido.Entities.Seminar;
using Aikido.Services.DatabaseServices.Seminar;
using Aikido.Services.DatabaseServices.StatisticService;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Aikido.Services.ApplicationServices
{
    public class StatisticApplicationService
    {
        private readonly IStatisticDbService _statisticService;
        private readonly ISeminarDbService _seminarDbService;

        #region Attendance

        public StatisticApplicationService(IStatisticDbService statisticService,
            ISeminarDbService seminarDbService)
        {
            _statisticService = statisticService;
            _seminarDbService = seminarDbService;
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
            var currentRetention = await CalculateYearRetention(year, filter) ?? 0;
            var previousRetention = await CalculateYearRetention(year - 1, filter);


            double? diff = null;

            if (previousRetention != null)
                diff = Math.Round(currentRetention - previousRetention.Value, 2);

                return new StatisticMetricDto(
                    Math.Round(currentRetention, 2),
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
                    date.ToString("dd.MM.yyyy H:mm:ss", CultureInfo.GetCultureInfo("ru-RU"))
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

        #region Seminars

        public async Task<StatisticMetricDto> GetAverageSeminarParticipationPercent(List<long> ids)
        {
            var avg = (await GetSeminarParticipationPercent(ids)).Average(s => s.Value);
            return new StatisticMetricDto(avg);
        }

        public async Task<List<SeminarStatisticMetricDto>> GetSeminarParticipationPercent(List<long> ids)
        {
            var result = new List<SeminarStatisticMetricDto>();

            var seminarStat = await _statisticService.GetSeminarMemberCount(ids);

            foreach (var pair in seminarStat)
            {
                var seminar = await _seminarDbService.GetByIdOrThrowException(pair.Key);

                var (currentPercent, totalCount) =
                    await CalculateSeminarPercent(seminar, pair.Value);

                var previousSeminar = await _seminarDbService.GetPreviousSeminar(seminar.Id);

                var previousPercent = await GetPreviousPercent(previousSeminar, seminarStat);

                var dynamic = previousPercent.HasValue
                    ? currentPercent - previousPercent
                    : null;

                result.Add(new SeminarStatisticMetricDto(
                    seminar,
                    new StatisticMetricDto(currentPercent, dynamic),
                    pair.Value,
                    totalCount
                ));
            }

            return result;
        }

        public async Task<StatisticMetricDto> GetAverageSeminarCertificationPercent(List<long> ids)
        {
            var data = await GetSeminarCertificationPercent(ids);
            var avg = data.Average(x => x.Value);

            return new StatisticMetricDto(avg);
        }

        public async Task<List<SeminarStatisticMetricDto>> GetSeminarCertificationPercent(List<long> ids)
        {
            var result = new List<SeminarStatisticMetricDto>();

            var totalStat = await _statisticService.GetSeminarMemberCount(ids);
            var certifiedStat = await _statisticService.GetSeminarCertificatedMembers(ids);

            foreach (var pair in totalStat)
            {
                var seminar = await _seminarDbService.GetByIdOrThrowException(pair.Key);

                var totalCount = pair.Value;

                var certifiedCount = certifiedStat.ContainsKey(pair.Key)
                    ? certifiedStat[pair.Key]
                    : 0;

                var percent = CalculatePercent(certifiedCount, totalCount);

                var previousSeminar = await _seminarDbService.GetPreviousSeminar(seminar.Id);

                double? previousPercent = null;

                if (previousSeminar != null)
                {
                    var prevTotal = totalStat.ContainsKey(previousSeminar.Id)
                        ? totalStat[previousSeminar.Id]
                        : _statisticService.GetSeminarMemberCount(previousSeminar.Id);

                    var prevCertified = certifiedStat.ContainsKey(previousSeminar.Id)
                        ? certifiedStat[previousSeminar.Id]
                        : 0;

                    previousPercent = CalculatePercent(prevCertified, prevTotal);
                }

                double? dynamic = previousPercent.HasValue
                    ? percent - previousPercent.Value
                    : null;

                result.Add(new SeminarStatisticMetricDto(
                    seminar,
                    new StatisticMetricDto(percent, dynamic),
                    certifiedCount,
                    totalCount
                ));
            }

            return result;
        }

        public async Task<StatisticMetricDto> GetAverageSeminarMoneyIncome(List<long> ids)
        {
            var data = await GetSeminarMoneyIncome(ids);
            var avg = data.Average(x => x.Value);

            return new StatisticMetricDto(avg);
        }

        public async Task<List<SeminarStatisticMetricDto>> GetSeminarMoneyIncome(List<long> ids)
        {
            var result = new List<SeminarStatisticMetricDto>();

            var incomeStat = await _statisticService.GetSeminarMoneyIncome(ids);

            foreach (var pair in incomeStat)
            {
                var seminar = await _seminarDbService.GetByIdOrThrowException(pair.Key);

                decimal income = pair.Value;

                var previousSeminar = await _seminarDbService.GetPreviousSeminar(seminar.Id);

                decimal? previousIncome = null;

                if (previousSeminar != null)
                {
                    if (incomeStat.ContainsKey(previousSeminar.Id))
                    {
                        previousIncome = incomeStat[previousSeminar.Id];
                    }
                    else
                    {
                        var prevDict = await _statisticService.GetSeminarMoneyIncome(
                            new List<long> { previousSeminar.Id });

                        previousIncome = prevDict.ContainsKey(previousSeminar.Id)
                            ? prevDict[previousSeminar.Id]
                            : 0;
                    }
                }

                double? dynamic = previousIncome.HasValue
                    ? (double)(income - previousIncome.Value)
                    : null;

                result.Add(new SeminarStatisticMetricDto(
                    seminar,
                    new StatisticMetricDto((double)income, dynamic),
                    0,
                    0
                ));
            }

            return result;
        }

        private async Task<(double percent, int totalCount)> CalculateSeminarPercent(
            SeminarEntity seminar,
            int count)
        {
            var total = await _statisticService.GetActiveMembersCountAt(seminar.Date);
            var percent = CalculatePercent(count, total);

            return (percent, total);
        }

        private async Task<double?> GetPreviousPercent(
            SeminarEntity? previousSeminar,
            Dictionary<long, int> cache)
        {
            if (previousSeminar == null)
                return null;

            var count = GetSeminarCount(previousSeminar.Id, cache);
            var total = await _statisticService.GetActiveMembersCountAt(previousSeminar.Date);

            return CalculatePercent(count, total);
        }

        private int GetSeminarCount(long seminarId, Dictionary<long, int> cache)
        {
            if (cache.TryGetValue(seminarId, out var count))
                return count;

            return _statisticService.GetSeminarMemberCount(seminarId);
        }

        private double CalculatePercent(int count, int total)
        {
            if (total == 0)
            {
                return 0;
            }
            return Math.Round((double)count / total, 4) * 100;
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
                new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToString("dd.MM.yyyy H:mm:ss", CultureInfo.GetCultureInfo("ru-RU"))));

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
                    new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc).ToString("dd.MM.yyyy H:mm:ss", CultureInfo.GetCultureInfo("ru-RU"))));
            }

            return metrics;
        }
    }
}
