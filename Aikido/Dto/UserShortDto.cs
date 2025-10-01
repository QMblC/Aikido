using Aikido.Entities;

namespace Aikido.Dto
{
    public class UserShortDto : DtoBase
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string>? ClubNames { get; set; }
        public List<string>? GroupNames { get; set; }
        public string? Role { get; set; }
        public string? Grade { get; set; }
        public string? PhoneNumber { get; set; }
        public string? City { get; set; }

        public UserShortDto() { }

        public UserShortDto(UserEntity user)
        {
            Id = user.Id;
            Name = user.FullName;
            Role = user.Role.ToString();
            Grade = user.Grade.ToString();
            PhoneNumber = user.PhoneNumber;
            City = user.City;
            ClubNames = new List<string>();
            GroupNames = new List<string>();
        }
    }
}