using Aikido.Entities;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Dto
{
    public class ClubDto : DtoBase
    {
        public long? Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? Description { get; set; }
        public long? HeadCoachId { get; set; }
        public string? HeadCoachName { get; set; }
        public DateTime? FoundedDate { get; set; }
        public bool IsActive { get; set; } = true;
        public int MemberCount { get; set; }
        public int GroupCount { get; set; }

        public ClubDto() { }

        public ClubDto(ClubEntity club)
        {
            Id = club.Id;
            Name = club.Name;
            City = club.City;
            Address = club.Address;
            PhoneNumber = club.PhoneNumber;
            Email = club.Email;
            Website = club.Website;
            Description = club.Description;
            HeadCoachId = club.HeadCoachId;
            HeadCoachName = club.HeadCoach?.FullName;
            FoundedDate = club.FoundedDate;
            IsActive = club.IsActive;
            MemberCount = club.UserClubs?.Count(uc => uc.IsActive) ?? 0;
            GroupCount = club.Groups?.Count(g => g.IsActive) ?? 0;
        }
    }
}