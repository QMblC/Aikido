using Aikido.AdditionalData.Enums;
using Aikido.Entities;

namespace Aikido.Dto.Groups
{
    public class GroupShortDto : DtoBase, IGroupDto
    {
        public string Name { get; set; }
        public string? TechnicalName { get; set; }
        public long? ClubId { get; set; }
        public string? ClubName { get; set; }
        public long? MainCoachId { get; set; }
        public string? AgeGroup { get; set; }
        public int MemberCount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public GroupShortDto(GroupDto group)
        {
            Id = group.Id;
            Name = group.Name ?? string.Empty;
            ClubId = group.ClubId;
            ClubName = group.Name;
            MainCoachId = group.MainCoachId;
            AgeGroup = group.AgeGroup;
            MemberCount = group.MemberCount.Value;

            CreatedAt = group.CreatedAt;
            ClosedAt = group.ClosedAt;
        }

        public GroupShortDto(GroupEntity group)
        {
            Id = group.Id;
            Name = group.Name ?? string.Empty;
            ClubId = group.ClubId;
            ClubName = group.Club?.Name;
            MainCoachId = group.MainCoachId;
            AgeGroup = EnumParser.ConvertEnumToString(group.AgeGroup);
            MemberCount = group.UserMemberships?.Count(um => um.RoleInGroup == Role.User) ?? 0;

            CreatedAt = group.CreatedAt;
            ClosedAt = group.ClosedAt;
        }
    }
}
