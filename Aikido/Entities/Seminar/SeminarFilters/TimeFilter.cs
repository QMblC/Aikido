namespace Aikido.Entities.Seminar.SeminarFilters
{
    public class TimeFilter
    {
        public bool IsUpcoming { get; set; } = true;
        public bool IsPast { get; set; } = true;

        public TimeFilter() { }
    }
}
