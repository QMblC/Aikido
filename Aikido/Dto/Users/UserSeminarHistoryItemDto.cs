using Aikido.Entities.Seminar;

namespace Aikido.Dto.Users
{
    public class UserSeminarHistoryItemDto
    {
        public string SeminarName { get; set; }
        public  DateTime SeminarDate { get; set; }
        public string SeminarLocation { get; set; }

        public UserSeminarHistoryItemDto() { }

        public UserSeminarHistoryItemDto(SeminarEntity seminar)
        {
            SeminarName = seminar.Name;
            SeminarDate = seminar.Date;
            SeminarLocation = seminar.Location ?? "-";
        }
    }
}
