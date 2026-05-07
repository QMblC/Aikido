namespace Aikido.Dto.Attendance
{
    public class AttendanceUpdateDto
    {
        public List<AttendanceCreationDto> ToCreate { get; set; }
        public List<long> ToDelete { get; set; }
    }
}
