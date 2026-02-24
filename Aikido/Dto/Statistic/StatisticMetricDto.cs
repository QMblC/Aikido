namespace Aikido.Dto.Statistic
{
    public class StatisticMetricDto
    {
        public double Value { get; set; }
        public double? Dynamic { get; set; }
        public string? Description { get; set; }

        public StatisticMetricDto(double value)
        {
            Value = value;
        }

        public StatisticMetricDto(double value, double dynamic) : this(value)
        {
            Dynamic = dynamic;
        }

        public StatisticMetricDto(double value, double dynamic, string description) : this(value, dynamic)
        {
            Description = description;
        }
    }
}
