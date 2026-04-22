namespace Aikido.Dto.Attendance
{
    public class ActivePeriodDto
    {
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }

        public ActivePeriodDto() { }

        public ActivePeriodDto(DateTime start)
        {
            Start = start;
        }
        public ActivePeriodDto(DateTime start, DateTime? end) : this(start)
        {
            End = end;
        }
    }
}
