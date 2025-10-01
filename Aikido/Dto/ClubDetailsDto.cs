using Aikido.Entities;

namespace Aikido.Dto
{
    public class ClubDetailsDto : DtoBase
    {
        public long? Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? Description { get; set; }
        public long? HeadCoachId { get; set; }
        public string? HeadCoachName { get; set; }
        public DateTime? FoundedDate { get; set; }
        public bool? IsActive { get; set; }
        public List<GroupDto>? Groups { get; set; } = new();
        public List<UserShortDto>? Members { get; set; } = new();
        public int? TotalMembers { get; set; }

        public ClubDetailsDto() { }

        public ClubDetailsDto(ClubEntity club)
        {
            Id = club.Id;
            Name = club.Name;
            City = club.City;
            Address = club.Address;
            PhoneNumber = club.PhoneNumber;
            Email = club.Email;
            Website = club.Website;
            Description = club.Description;
            HeadCoachId = club.ManagerId;
            HeadCoachName = club.Manager?.FullName;
            FoundedDate = club.FoundedDate;
            IsActive = club.IsActive;
        }

        public ClubDetailsDto(ClubEntity club, List<GroupEntity> groups, List<UserClub> members) : this(club)
        {
            Groups = groups.Where(g => g.IsActive).Select(g => new GroupDto(g)).ToList();
            Members = members.Where(m => m.IsActive && m.User != null)
                            .Select(m => new UserShortDto(m.User!))
                            .ToList();
            TotalMembers = Members.Count;
        }
    }
}