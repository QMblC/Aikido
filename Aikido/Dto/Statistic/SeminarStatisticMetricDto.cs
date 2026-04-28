using Aikido.Entities.Seminar;

namespace Aikido.Dto.Statistic
{
    public class SeminarStatisticMetricDto
    {
        public long SeminarId { get; set; }
        public string SeminarName { get; set; }
        public double? Value { get; set; }
        public double? Dynamic { get; set; }

        public int Count { get; set; }
        public int TotalCount { get; set; }

        public SeminarStatisticMetricDto() { }

        public SeminarStatisticMetricDto(SeminarEntity seminar, StatisticMetricDto metric, int count, int totalCount)
        {
            SeminarId = seminar.Id;
            SeminarName = seminar.Name;
            Value = metric.Value;
            Dynamic = metric.Dynamic;
            Count = count;
            TotalCount = totalCount;
        }
    }
}
