using Aikido.AdditionalData;
using Aikido.Dto.Seminars.Members;
using Aikido.Entities.Users;

namespace Aikido.Entities.Seminar.SeminarMemberRequest
{
    public class SeminarMemberRequestEntity : SeminarMemberEntity
    {
        public bool IsMemberReadyForAppliement { get; set; }

        //public SeminarMemberRequestEntity(long coachId,
        //    SeminarEntity seminar,
        //    UserMembershipEntity userMemberShip,
        //    SeminarMemberRequestCreationDto seminarMember,
        //    SeminarMemberStatus status = SeminarMemberStatus.None) : base(coachId, seminar, userMemberShip, seminarMember, status)
        //{
        //    IsMemberReadyForAppliement = seminarMember.IsMemberReadyForAppliement;
        //}

        public new void UpdateData(
            long coachId,
            SeminarEntity seminar,
            UserMembershipEntity userMembership,
            SeminarMemberRequestCreationDto seminarMember,
            SeminarMemberStatus status = SeminarMemberStatus.None) 
        {
            SeminarId = seminar.Id;
            UserId = seminarMember.UserId;
            TrainingGroupId = userMembership.GroupId;
            SeminarGroupId = seminarMember.SeminarGroupId;
            Status = status;
            OldGrade = userMembership.User.Grade;
            CertificationGrade = seminarMember.CertificationGrade != null
                ? EnumParser.ConvertStringToEnum<Grade>(seminarMember.CertificationGrade) : Grade.None;

            CreatorId = coachId;

            IsMemberReadyForAppliement = seminarMember.IsMemberReadyForAppliement;
        }
    }
}
