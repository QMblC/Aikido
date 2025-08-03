using Aikido.Dto;
using Aikido.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class ClubEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long ManagerId { get; set; }
        public UserEntity Manager { get; set; }

        public string? Name { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }

        public List<GroupEntity> Groups { get; set; } = new();

        public void UpdateFromJson(ClubDto clubNewData)
        {
            if (clubNewData.Name != null)
                Name = clubNewData.Name;

            if (clubNewData.City != null)
                City = clubNewData.City;

            if (clubNewData.Address != null)
                Address = clubNewData.Address;
        }
    }
}