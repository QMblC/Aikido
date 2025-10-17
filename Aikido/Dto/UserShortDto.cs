using Aikido.Entities;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Aikido.Dto
{
    public class UserShortDto : DtoBase//fix FullName
    {
        public long? Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public List<string>? ClubNames { get; set; }
        public List<string>? GroupNames { get; set; }
        public string? Role { get; set; }
        public string? Grade { get; set; }
        public string? PhoneNumber { get; set; }
        public string? City { get; set; }

        public string FullName => $"{LastName} {FirstName} {MiddleName}";

        public UserShortDto() { }

        public UserShortDto(UserEntity user)
        {
            Id = user.Id;
            LastName = user.LastName;
            FirstName = user.FirstName;
            MiddleName = user.SecondName;
            Role = user.Role.ToString();
            Grade = user.Grade.ToString();
            PhoneNumber = user.PhoneNumber;
            City = user.City;
            ClubNames = new List<string>();
            GroupNames = new List<string>();
        }
    }
}