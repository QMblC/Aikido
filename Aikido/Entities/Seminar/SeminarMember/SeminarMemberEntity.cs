using Aikido.AdditionalData.Enums;
using Aikido.Dto.Seminars;
using Aikido.Dto.Seminars.Members.Creation;
using Aikido.Entities.Seminar.SeminarMemberRequest;
using Aikido.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Seminar.SeminarMember
{
    public class SeminarMemberEntity : IDbEntity, ISeminarMemberData
    {
        [Key]
        public long Id { get; set; }

        public long SeminarId { get; set; }
        public virtual SeminarEntity? Seminar { get; set; }

        public long UserId { get; set; }
        public virtual UserEntity? User { get; set; }

        public long? GroupId { get; set; }
        public virtual GroupEntity Group { get; set; }

        public long? ClubId { get; set; }
        public virtual ClubEntity Club { get; set; }

        public long? CoachId { get; set; }
        public virtual UserEntity? Coach { get; set; }

        public long? SeminarGroupId { get; set; }
        public virtual SeminarGroupEntity? SeminarGroup { get; set; }

        public SeminarMemberStatus Status { get; set; } = SeminarMemberStatus.None;
        public Grade OldGrade { get; set; }
        public Grade? CertificationGrade { get; set; }

        public long? ManagerId { get; set; }
        public virtual UserEntity? Manager { get; set; }

        public string? Note { get; set; }

        public SeminarMemberEntity() { }

        public SeminarMemberEntity(
            long coachId,
            SeminarEntity seminar,
            UserMembershipEntity userMemberShip, 
            SeminarMemberCreationDto seminarMember,
            SeminarMemberStatus status = SeminarMemberStatus.None)
        {
            UpdateData(coachId, seminar, userMemberShip, seminarMember);
        }

        public SeminarMemberEntity(SeminarMemberManagerRequestEntity member)
        {
            SeminarId = member.SeminarId;
            UserId = member.UserId;
            GroupId = member.GroupId;
            ClubId = member.ClubId;
            SeminarGroupId = member.SeminarGroupId;
            Status = member.CertificationGrade == Grade.None ? SeminarMemberStatus.Training : SeminarMemberStatus.Certified;
            OldGrade = member.OldGrade;
            CertificationGrade = member.CertificationGrade;
            CoachId = member.CoachId;
            ManagerId = member.ManagerId;
            Note = member.Note;
        }

        public void UpdateData(
            long coachId,
            SeminarEntity seminar,
            UserMembershipEntity userMembership,
            SeminarMemberCreationDto seminarMember,
            SeminarMemberStatus status = SeminarMemberStatus.None)
        {
            SeminarId = seminar.Id;
            UserId = seminarMember.UserId;
            GroupId = seminarMember.GroupId;
            ClubId = userMembership.ClubId;
            SeminarGroupId = seminarMember.SeminarGroupId;
            Status = status;
            OldGrade = userMembership.User.Grade;
            CertificationGrade = seminarMember.CertificationGrade != null
                ? EnumParser.ConvertStringToEnum<Grade>(seminarMember.CertificationGrade) : Grade.None;

            CoachId = coachId;
            Note = seminarMember.Note;
        }

        public void UpdateData(SeminarEntity seminar,
            UserMembershipEntity userMembership,
            SeminarMemberCreationDto seminarMember)
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

            Status = CertificationGrade == Grade.None ? SeminarMemberStatus.Training : SeminarMemberStatus.Certified;

            CoachId = seminarMember.CoachId;
            ManagerId = userMembership.Club?.ManagerId;
            Note = seminarMember.Note;
        }

        public SeminarMemberEntity(SeminarEntity seminar, UserMembershipEntity userMembership)
        {
            SeminarId = seminar.Id;
            UserId = userMembership.UserId;
            ClubId = userMembership.ClubId;
            GroupId = userMembership.GroupId;

            OldGrade = userMembership.User != null
                ? userMembership.User.Grade
                : Grade.None;
            CertificationGrade = Grade.None;

            Status = SeminarMemberStatus.Training;

            CoachId = userMembership.Group?.UserMemberships.FirstOrDefault(um => um.RoleInGroup == Role.Coach)?.UserId;
            ManagerId = userMembership.Club?.ManagerId;
        }
    }
}