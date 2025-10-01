using Aikido.AdditionalData;
using Aikido.Dto;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class GroupEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public long? CoachId { get; set; }
        public virtual UserEntity? Coach { get; set; }

        public long? ClubId { get; set; }
        public virtual ClubEntity? Club { get; set; }

        public string? Name { get; set; }
        public AgeGroup AgeGroup { get; set; } = AgeGroup.Adult;
        public int MaxMembers { get; set; } = 30;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public string? Description { get; set; }
        public Grade MinGrade { get; set; } = Grade.None;
        public Grade MaxGrade { get; set; } = Grade.Dan10;

        // Навигационное свойство для many-to-many связи с пользователями
        public virtual ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
        public virtual ICollection<ScheduleEntity> Schedules { get; set; } = new List<ScheduleEntity>();

        public GroupEntity() { }

        public void UpdateFromJson(GroupDto groupNewData)
        {
            if (groupNewData.CoachId != null)
                CoachId = (long)groupNewData.CoachId;
            if (groupNewData.ClubId != null)
                ClubId = (long)groupNewData.ClubId;
            if (!string.IsNullOrEmpty(groupNewData.Name))
                Name = groupNewData.Name;
            if (!string.IsNullOrEmpty(groupNewData.AgeGroup))
                AgeGroup = EnumParser.ConvertStringToEnum<AgeGroup>(groupNewData.AgeGroup);
            IsActive = groupNewData.IsActive;
            UpdatedDate = DateTime.UtcNow;
        }

        public void UpdateFromJson(GroupInfoDto groupNewData)
        {
            if (groupNewData.CoachId != null)
                CoachId = (long)groupNewData.CoachId;
            if (groupNewData.ClubId != null)
                ClubId = (long)groupNewData.ClubId;
            if (!string.IsNullOrEmpty(groupNewData.Name))
                Name = groupNewData.Name;
            if (!string.IsNullOrEmpty(groupNewData.AgeGroup))
                AgeGroup = EnumParser.ConvertStringToEnum<AgeGroup>(groupNewData.AgeGroup);
            IsActive = groupNewData.IsActive;
            UpdatedDate = DateTime.UtcNow;
        }
    }
}