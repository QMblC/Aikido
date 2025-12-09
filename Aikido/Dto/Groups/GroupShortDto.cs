using Aikido.AdditionalData;
using Aikido.Entities;

namespace Aikido.Dto.Groups
{
    public class GroupShortDto : DtoBase, IGroupDto
    {
        public string Name { get; set; }
        public string? TechnicalName { get; set; }
        public long? ClubId { get; set; }
        public string? ClubName { get; set; }
        public string? AgeGroup { get; set; }
        public int MemberCount { get; set; }

        public GroupShortDto(GroupDto group)
        {
            Id = group.Id;
            Name = group.Name ?? string.Empty;
            ClubId = group.ClubId;
            ClubName = group.Name;
            AgeGroup = group.AgeGroup;
            MemberCount = group.MemberCount.Value;
        }

        public GroupShortDto(GroupEntity group)
        {
            Id = group.Id;
            Name = group.Name ?? string.Empty;
            ClubId = group.ClubId;
            ClubName = group.Club?.Name;
            AgeGroup = EnumParser.ConvertEnumToString(group.AgeGroup);
            MemberCount = group.UserMemberships?.Count(um => um.RoleInGroup == Role.User) ?? 0;
        }
    }
}
