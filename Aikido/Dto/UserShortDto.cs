using Aikido.Entities;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Aikido.Dto
{
    public class UserShortDto : DtoBase
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string? Grade { get; set; }
        public byte[]? Photo { get; set; } = [];


        public string FullName => $"{LastName} {FirstName}";

        public UserShortDto() { }

        public UserShortDto(UserDto user)
        {
            Id = user.Id;
            LastName = user.LastName;
            FirstName = user.FirstName;
            Grade = user.Grade;
            Photo = user.Photo;
        }

        public UserShortDto(UserEntity user)
        {
            Id = user.Id;
            LastName = user.LastName;
            FirstName = user.FirstName;
            Grade = user.Grade.ToString();
            Photo = user.Photo;
        }
    }
}