namespace Aikido.Dto
{
    public class AttendanceDto
    {
        public long? Id { get; set; }
        public long? UserId { get; set; }
        public long? GroupId { get; set; }
        public DateTime? VisitDate { get; set; }
    }
}
