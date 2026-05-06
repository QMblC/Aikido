using Aikido.Entities.Clubs;

namespace Aikido.Dto.Clubs
{
    public class ClubShortDto : DtoBase, IClubDto
    {
        public string Name { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public long? ManagerId { get; set; }
        public string? ManagerName { get; set; }

        public ClubShortDto()
        {

        }

        public ClubShortDto(ClubEntity club)
        {
            Id = club.Id;
            Name = club.Name;
            City = club.City;
            Address = club.Address;
            ManagerId = club.ManagerId;
            ManagerName = club.Manager?.FullName;
        }
    }
}
