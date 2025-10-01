using Aikido.Entities;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Dto
{
    public class GroupDto : DtoBase
    {
        public long? Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public long? CoachId { get; set; }
        public string? CoachName { get; set; }
        public long? ClubId { get; set; }
        public string? ClubName { get; set; }
        public string? AgeGroup { get; set; }
        public int MemberCount { get; set; }
        public int MaxMembers { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedDate { get; set; }
        public string? Description { get; set; }
        public string? MinGrade { get; set; }
        public string? MaxGrade { get; set; }

        public GroupDto() { }

        public GroupDto(GroupEntity group)
        {
            Id = group.Id;
            Name = group.Name;
            CoachId = group.CoachId;
            CoachName = group.Coach?.FullName;
            ClubId = group.ClubId;
            ClubName = group.Club?.Name;
            AgeGroup = group.AgeGroup.ToString();
            MemberCount = group.UserGroups?.Count(ug => ug.IsActive) ?? 0;
            MaxMembers = group.MaxMembers;
            IsActive = group.IsActive;
            CreatedDate = group.CreatedDate;
            Description = group.Description;
            MinGrade = group.MinGrade.ToString();
            MaxGrade = group.MaxGrade.ToString();
        }
    }
}