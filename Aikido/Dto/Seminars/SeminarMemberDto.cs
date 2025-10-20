using Aikido.Entities.Seminar;

namespace Aikido.Dto.Seminars
{
    public class SeminarMemberDto : DtoBase
    {
        public long UserId { get; set; }
        public string? UserFullName { get; set; } = string.Empty;

        public long? SeminarId { get; set; }
        public string? SeminarName { get; set; } = string.Empty;
        public DateTime? SeminarDate { get; set; }

        public long? SeminarGroupId { get; set; }
        public string? SeminarGroupName { get; set; }

        public string? OldGrade { get; set; } = string.Empty;
        public string? CertificationGrade { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        

        public SeminarMemberDto() { }

        public SeminarMemberDto(SeminarMemberEntity seminarMember)
        {
            Id = seminarMember.Id;
            UserId = seminarMember.UserId;
            UserFullName = seminarMember.User?.FullName ?? string.Empty;

            SeminarId = seminarMember.SeminarId;
            SeminarName = seminarMember.Seminar?.Name ?? string.Empty;
            SeminarDate = seminarMember.Seminar?.Date;

            SeminarGroupId = seminarMember.GroupId;
            SeminarGroupName = seminarMember.Group?.Name;

            OldGrade = seminarMember.OldGrade.ToString();
            CertificationGrade = seminarMember.CertificationGrade.ToString();
            Status = seminarMember.Status.ToString();
        }
    }
}
