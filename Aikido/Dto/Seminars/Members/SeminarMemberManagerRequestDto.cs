using Aikido.Entities.Seminar;
using Aikido.Entities.Seminar.SeminarMemberRequest;
using Aikido.Entities.Users;

namespace Aikido.Dto.Seminars.Members
{
    public class SeminarMemberManagerRequestDto : SeminarMemberDto
    {
        public bool IsMemberReadyForAppliement { get; set; } = false;

        public SeminarMemberManagerRequestDto(SeminarMemberManagerRequestEntity seminarMember) : base(seminarMember) 
        {
            IsMemberReadyForAppliement = seminarMember.IsConfirmed;
        }

        public SeminarMemberManagerRequestDto(
            UserMembershipEntity membership,
            SeminarEntity seminar,
            bool isAnnualFeePayed = false)
        {
            UserId = membership.UserId;
            UserFullName = membership.User?.FullName ?? string.Empty;

            SeminarId = seminar.Id;
            SeminarName = seminar.Name;
            SeminarDate = seminar.Date;

            GroupId = membership.GroupId;
            GroupName = membership.Group?.Name;

            OldGrade = EnumParser.ConvertEnumToString(membership.User.Grade);

            SeminarPriceInRubles = seminar.SeminarPriceInRubles;
            BudoPassportPriceInRubles = membership.User.HasBudoPassport ? null : seminar.BudoPassportPriceInRubles;
            AnnualFeePriceInRubles = isAnnualFeePayed ? null : seminar.AnnualFeePriceInRubles;

            IsMemberReadyForAppliement = false;
        }
    }
}
