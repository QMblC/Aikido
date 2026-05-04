using Aikido.Dto.Clubs;
using Aikido.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Clubs
{
    public class ClubEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? Description { get; set; }
        public long? ManagerId { get; set; }
        public virtual UserEntity? Manager { get; set; }
        public DateTime? FoundedDate { get; set; }

        public virtual ICollection<GroupEntity> Groups { get; set; } = new List<GroupEntity>();
        public virtual ICollection<UserMembershipEntity> UserMemberships { get; set; } = new List<UserMembershipEntity>();
        public virtual ICollection<ClubStaffEntity> Staff { get; set; } = new List<ClubStaffEntity>();

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }

        public ClubEntity() { }

        public ClubEntity(IClubDto clubData)
        {
            UpdateFromJson(clubData);
        }

        public void UpdateFromJson(IClubDto clubData)
        {
            if (!string.IsNullOrEmpty(clubData.Name))
                Name = clubData.Name;
            City = clubData.City;
            Address = clubData.Address;
        }
    }
}