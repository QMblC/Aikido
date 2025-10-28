using Aikido.AdditionalData;
using Aikido.Dto.Seminars;
using Aikido.Entities;
using Aikido.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Dto.Users
{
    public class UserDto : UserShortDto, IUserDto
    {
        public string? Role { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }

        public string? MiddleName { get; set; }

        public new string FullName => $"{LastName} {FirstName} {MiddleName}";
        public string? Sex { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Education { get; set; }
        public List<SeminarMemberDto>? Certifications { get; set; }
        public bool? HasBudoPassport { get; set; }

        public string? City { get; set; }
        public string? ParentFullName { get; set; }
        public string? ParentPhoneNumber { get; set; }
        public DateTime? RegistrationDate { get; set; }

        public DateTime? CreationDate { get; set; }

        public List<UserMembershipDto>? UserMembershipDtos { get; set; } = new();

        public UserDto() { }

        public UserDto(UserEntity user)
        {
            Id = user.Id;
            Role = user.Role.ToString();
            Login = user.Login;
            Password = user.Password;
            LastName = user.LastName;
            FirstName = user.FirstName;
            MiddleName = user.MiddleName;
            Sex = user.Sex.ToString();
            Photo = user.Photo;
            PhoneNumber = user.PhoneNumber;
            Birthday = user.Birthday;
            Grade = user.Grade.ToString();
            Education = user.Education.ToString();
            Certifications = user.Certifications?.Select(sm => new SeminarMemberDto(sm))
                .ToList();
            HasBudoPassport = user.HasBudoPassport;
            City = user.City;
            ParentFullName = user.ParentFullName;
            ParentPhoneNumber = user.ParentPhoneNumber;
            RegistrationDate = user.RegistrationDate;
            CreationDate = user.CreationDate;
        }

        public UserDto(UserEntity user, List<UserMembershipEntity> userMemberships) : this(user)
        {
            UserMembershipDtos = userMemberships.Select(um =>  new UserMembershipDto(um)).ToList();
        }
    }
}