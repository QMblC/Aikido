using Aikido.AdditionalData.Enums;
using Aikido.Dto.ExclusionDates;
using Aikido.Dto.Schedule;
using Aikido.Dto.Users;
using Aikido.Entities;

namespace Aikido.Dto.Groups
{
    public class GroupInfoDto : DtoBase
    {
        public long? Id { get; set; }
        public string? Name { get; set; } = string.Empty;

        public List<UserShortDto> Coaches { get; set; }

        public long? ClubId { get; set; }
        public string? ClubName { get; set; }
        public string? AgeGroup { get; set; }
        public List<UserShortDto>? GroupMembers { get; set; } = new();
        public DateTime? CreatedDate { get; set; }
        public List<ScheduleDto>? Schedule { get; set; } = new();     
        public List<ExclusionDateDto>? ExclusionDates { get; set; } = new();
        public string? Description { get; set; }
        public int? MaxMembers { get; set; }
        public string? MinGrade { get; set; }
        public string? MaxGrade { get; set; }

        public GroupInfoDto() { }

        public GroupInfoDto(GroupEntity group)
        {
            Id = group.Id;
            Name = group.Name;
            
            Coaches = group.UserMemberships
                .Where(um => um.RoleInGroup == Role.Coach)
                .Select(um => new UserShortDto(um.User))
                .ToList();

            ClubId = group.ClubId;
            ClubName = group.Club?.Name;
            AgeGroup = EnumParser.ConvertEnumToString(group.AgeGroup);
            CreatedDate = group.CreatedDate;
            Schedule = group.Schedule
                .Select(s => new ScheduleDto(s))
                .ToList();
            ExclusionDates = group.ExclusionDates
                .Select(e => new ExclusionDateDto(e))
                .ToList();
            Description = group.Description;
            MaxMembers = group.MaxMembers;
            MinGrade = EnumParser.ConvertEnumToString(group.MinGrade);
            MaxGrade = EnumParser.ConvertEnumToString(group.MaxGrade);
        }
    }
}