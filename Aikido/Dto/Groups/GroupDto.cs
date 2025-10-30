using Aikido.AdditionalData;
using Aikido.Dto.ExclusionDates;
using Aikido.Dto.Schedule;
using Aikido.Dto.Users;
using Aikido.Entities;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Dto.Groups
{
    public class GroupDto : DtoBase, IGroupDto
    {
        public long? Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<UserShortDto> Coaches { get; set; }

        public long? ClubId { get; set; }
        public string? ClubName { get; set; }
        public string? AgeGroup { get; set; }
        public int? MemberCount { get; set; }
        public int? MaxMembers { get; set; }
        public bool? IsActive { get; set; } = true;
        public DateTime? CreatedDate { get; set; }
        public string? Description { get; set; }
        public string? MinGrade { get; set; }
        public string? MaxGrade { get; set; }

        public List<ScheduleDto>? Schedule { get; set; } = new();
        public List<ExclusionDateDto>? ExclusionDates { get; set; } = new();

        public GroupDto() { }

        public GroupDto(GroupEntity group)
        {
            Id = group.Id;
            Name = group.Name;

            Coaches = group.UserMemberships
                .Where(um => um.RoleInGroup == Role.Coach
                && um.UserId != null)
                .Select(um => new UserShortDto(um.User))
                .ToList();

            ClubId = group.ClubId;
            ClubName = group.Club?.Name;
            AgeGroup = group.AgeGroup.ToString();
            MemberCount = group.UserMemberships?.Count(um => um.RoleInGroup == Role.User) ?? 0;
            MaxMembers = group.MaxMembers;
            IsActive = group.IsActive;
            CreatedDate = group.CreatedDate;
            Description = group.Description;
            MinGrade = group.MinGrade.ToString();
            MaxGrade = group.MaxGrade.ToString();
            Schedule = group.Schedule
                .Select(s => new ScheduleDto(s))
                .ToList();
            ExclusionDates = group.ExclusionDates
                .Select(e => new ExclusionDateDto(e))
                .ToList();
        }
    }
}