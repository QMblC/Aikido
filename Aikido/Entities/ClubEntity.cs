using Aikido.Dto;
using Aikido.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
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
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        public virtual ICollection<GroupEntity> Groups { get; set; } = new List<GroupEntity>();
        public virtual ICollection<UserMembershipEntity> UserMemberships { get; set; } = new List<UserMembershipEntity>();

        public ClubEntity() { }

        public ClubEntity(ClubDto clubData)
        {
            UpdateFromJson(clubData);
        }

        public void UpdateFromJson(ClubDto clubData)
        {
            if (!string.IsNullOrEmpty(clubData.Name))
                Name = clubData.Name;
            City = clubData.City;
            Address = clubData.Address;
            PhoneNumber = clubData.PhoneNumber;
            Email = clubData.Email;
            Website = clubData.Website;
            Description = clubData.Description;
            ManagerId = clubData.ManagerId;
            FoundedDate = clubData.FoundedDate;
            if (clubData.IsActive.HasValue)
            {
                IsActive = clubData.IsActive.Value;
            }       
            else
            {
                IsActive = true;
            }
                UpdatedDate = DateTime.UtcNow;
        }
    }
}