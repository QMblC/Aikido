namespace Aikido.Dto.ExclusionDates
{
    public interface IExclusionDateDto
    {
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Type { get; set; }
        public string? Description { get; set; }
    }
}
