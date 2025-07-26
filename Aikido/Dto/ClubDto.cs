using Aikido.Entities;

namespace Aikido.Dto
{
    public class ClubDto : DtoBase
    {
        public long? Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Address { get; set; }

        public ClubDto() { }

        public ClubDto(ClubEntity clubEntity)
        {
            Id = clubEntity.Id;
            Name = clubEntity.Name;
            City = clubEntity.City;
            Address = clubEntity.Address;
        }
    }
}
