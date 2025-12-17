using Aikido.AdditionalData.Enums;
using Aikido.Dto.Seminars.Members.Creation;
using Aikido.Entities.Seminar.SeminarMember;
using Aikido.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Seminar.SeminarMemberRequest
{
    public class SeminarMemberManagerRequestEntity : IDbEntity, ISeminarMemberData
    {
        [Key]
        public long Id { get; set; }

        public long SeminarId { get; set; }
        public virtual SeminarEntity? Seminar { get; set; }

        public long UserId { get; set; }
        public virtual UserEntity? User { get; set; }

        public long? GroupId { get; set; }
        public virtual GroupEntity? Group { get; set; }

        public long? ClubId { get; set; }
        public virtual ClubEntity? Club { get; set; }

        public long? CoachId { get; set; }
        public virtual UserEntity? Coach { get; set; }

        public long? SeminarGroupId { get; set; }
        public virtual SeminarGroupEntity? SeminarGroup { get; set; }

        public Grade OldGrade { get; set; }
        public Grade? CertificationGrade { get; set; }

        public long? ManagerId { get; set; }
        public virtual UserEntity? Manager { get; set; }

        public string? Note { get; set; }

        public bool IsConfirmed { get; set; } = false;

        public SeminarMemberManagerRequestEntity() { }

        public SeminarMemberManagerRequestEntity(long coachId,
            SeminarEntity seminar,
            UserMembershipEntity userMemberShip,
            SeminarMemberRequestCreationDto seminarMember)
        {
            UpdateData(coachId, seminar, userMemberShip, seminarMember);
        }

        public SeminarMemberManagerRequestEntity(SeminarEntity seminar,
            UserMembershipEntity userMembership,
            SeminarMemberRequestCreationDto seminarMember)
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

            CoachId = userMembership.Group?.UserMemberships.FirstOrDefault(um => um.RoleInGroup == Role.Coach)?.UserId;
            ManagerId = userMembership.Club?.ManagerId;
        }

        public void UpdateData(SeminarEntity seminar,
            UserMembershipEntity userMembership,
            SeminarMemberRequestCreationDto seminarMember)
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
            ManagerId = userMembership.Club?.ManagerId;
            Note = seminarMember.Note;
        }

        public void UpdateData(
            long coachId,
            SeminarEntity seminar,
            UserMembershipEntity userMembership,
            SeminarMemberRequestCreationDto seminarMember) 
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
