using Aikido.AdditionalData.Enums;
using Aikido.Dto.Groups;
using Aikido.Entities.Clubs;
using Aikido.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class GroupEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long? ClubId { get; set; }
        public virtual ClubEntity? Club { get; set; }

        public long? MainCoachId { get; set; }
        public virtual UserEntity? MainCoach { get; set; }

        public string Name { get; set; }
        public string? TechnicalName { get; set; }
        public AgeGroup AgeGroup { get; set; } = AgeGroup.Adult;
        public int MaxMembers { get; set; } = 30;

        public string? Description { get; set; }
        public Grade MinGrade { get; set; } = Grade.None;
        public Grade MaxGrade { get; set; } = Grade.None;

        public virtual List<UserMembershipEntity> UserMemberships { get; set; } = new();
        public virtual List<ScheduleEntity> Schedule { get; set; } = new();
        public virtual List<ExclusionDateEntity> ExclusionDates { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }

        public GroupEntity() { }

        public GroupEntity(GroupCreationDto group)
        {
            UpdateFromJson(group);
        }

        public void UpdateFromJson(GroupCreationDto groupNewData)
        {
            if (groupNewData.ClubId != null)
                ClubId = (long)groupNewData.ClubId;
            if (!string.IsNullOrEmpty(groupNewData.Name))
                Name = groupNewData.Name;

            MainCoachId = groupNewData.MainCoachId;

            TechnicalName = groupNewData.TechnicalName;

            if (!string.IsNullOrEmpty(groupNewData.AgeGroup))
                AgeGroup = EnumParser.ConvertStringToEnum<AgeGroup>(groupNewData.AgeGroup);        
        }
    }
}