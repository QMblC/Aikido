using Aikido.AdditionalData;
using Aikido.Dto.Seminars.Members;
using Aikido.Entities.Seminar.SeminarMember;
using Aikido.Entities.Users;

namespace Aikido.Entities.Seminar.SeminarMemberRequest
{
    public class SeminarMemberManagerRequestEntity : SeminarMemberEntity
    {
        public bool IsConfirmed { get; set; } = false;


        public SeminarMemberManagerRequestEntity() { }

        public SeminarMemberManagerRequestEntity(long coachId,
            SeminarEntity seminar,
            UserMembershipEntity userMemberShip,
            SeminarMemberManagerRequestCreationDto seminarMember)
        {
            UpdateData(coachId, seminar, userMemberShip, seminarMember);
        }

        public SeminarMemberManagerRequestEntity(SeminarEntity seminar,
            UserMembershipEntity userMembership,
            SeminarMemberManagerRequestCreationDto seminarMember)
        {
            UpdateData(seminar, userMembership, seminarMember);
        }

        public SeminarMemberManagerRequestEntity(SeminarEntity seminar, UserMembershipEntity userMembership)
        {
            SeminarId = seminar.Id;
            UserId = userMembership.UserId;
            ClubId = userMembership.ClubId;
            GroupId = userMembership.GroupId;

            OldGrade = userMembership.User != null
                ? userMembership.User.Grade
                : Grade.None;
            CertificationGrade = Grade.None;

            CoachId = userMembership.Group?.UserMemberships.FirstOrDefault(um => um.RoleInGroup == Role.Coach)?.Id;
            ManagerId = userMembership.Club?.ManagerId;
        }

        public void UpdateData(SeminarEntity seminar,
            UserMembershipEntity userMembership,
            SeminarMemberManagerRequestCreationDto seminarMember)
        {
            SeminarId = seminar.Id;
            UserId = userMembership.UserId;

            ClubId = userMembership.ClubId;
            GroupId = userMembership.GroupId;

            SeminarGroupId = seminarMember.SeminarGroupId;
            OldGrade = userMembership.User != null 
                ? userMembership.User.Grade 
                : Grade.None;
            CertificationGrade = seminarMember.CertificationGrade != null
                ? EnumParser.ConvertStringToEnum<Grade>(seminarMember.CertificationGrade) : Grade.None;

            CoachId = seminarMember.CoachId;
            Note = seminarMember.Note;
        }

        public void UpdateData(
            long coachId,
            SeminarEntity seminar,
            UserMembershipEntity userMembership,
            SeminarMemberManagerRequestCreationDto seminarMember) 
        {
            SeminarId = seminar.Id;
            UserId = seminarMember.UserId;

            ClubId = userMembership.ClubId;
            GroupId = userMembership.GroupId;

            SeminarGroupId = seminarMember.SeminarGroupId;
            OldGrade = userMembership.User.Grade;
            CertificationGrade = seminarMember.CertificationGrade != null
                ? EnumParser.ConvertStringToEnum<Grade>(seminarMember.CertificationGrade) : Grade.None;

            CoachId = coachId;
            Note = seminarMember.Note;
        }
    }
}
