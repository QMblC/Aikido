using Aikido.AdditionalData;
using Aikido.Dto;
using Aikido.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class GroupEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public long? CoachId { get; set; }
        public UserEntity? Coach { get; set; }

        public long? ClubId { get; set; }
        public ClubEntity Club { get; set; }

        public string? Name { get; set; }
        public AgeGroup AgeGroup { get; set; } = AgeGroup.Adult;

        public List<ScheduleEntity> Schedules { get; set; } = new();
        public List<ExclusionDateEntity> ExclusionDates { get; set; } = new();

        public List<UserGroupDataEntity> UserGroupData { get; set; } = new();
        public List<AttendanceEntity> Attendances { get; set; } = [];

        public void UpdateFromJson(GroupDto groupNewData)
        {
            if (groupNewData.CoachId != null)
                CoachId = (long)groupNewData.CoachId;

            if (groupNewData.GroupMembers != null)
                UserIds = new List<long>(groupNewData.GroupMembers);

            if (groupNewData.ClubId != null)
                ClubId = (long)groupNewData.ClubId;

            if (!string.IsNullOrEmpty(groupNewData.Name))
                Name = groupNewData.Name;

            if (!string.IsNullOrEmpty(groupNewData.AgeGroup))
                AgeGroup = EnumParser.ConvertStringToEnum<AgeGroup>(groupNewData.AgeGroup);
        }

        public void UpdateFromJson(GroupInfoDto groupNewData)
        {
            if (groupNewData.CoachId != null)
                CoachId = (long)groupNewData.CoachId;

            if (groupNewData.GroupMembers != null)
                UserIds = new(groupNewData.GroupMembers.Select(u => u.Id));

            if (groupNewData.ClubId != null)
                ClubId = (long)groupNewData.ClubId;

            if (!string.IsNullOrEmpty(groupNewData.Name))
                Name = groupNewData.Name;

            if (!string.IsNullOrEmpty(groupNewData.AgeGroup))
                AgeGroup = EnumParser.ConvertStringToEnum<AgeGroup>(groupNewData.AgeGroup);
        }

        public void AddUser(long userId, Role role = Role.User)
        {
            if (role == Role.User)
            {
                UserIds.Add(userId);
            }
        }

        public void DeleteUser(long userId)
        {
            if (CoachId == userId)
            {
                CoachId = null;
            }
            UserIds.Remove(userId);
        }
    }
}