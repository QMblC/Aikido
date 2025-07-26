namespace Aikido.Dto
{
    public class AttendanceDto : DtoBase
    {
        public long? UserId { get; set; }
        public long? GroupId { get; set; }
        public DateTime? VisitDate { get; set; }
    }
}
