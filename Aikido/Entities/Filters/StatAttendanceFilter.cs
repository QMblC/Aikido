namespace Aikido.Entities.Filters
{
    public class StatAttendanceFilter
    {
        public List<string>? Cities { get; set; }
        public List<string>? Grades { get; set; }
        public List<long>? ClubIds { get; set; }
        public List<long>? GroupIds { get; set; }
        public long? CoachId { get; set; }

    }
}
