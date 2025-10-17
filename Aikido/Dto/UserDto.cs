using Aikido.AdditionalData;
using Aikido.Entities;
using Aikido.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Dto
{
    public class UserDto : DtoBase
    {
        public long? Id { get; set; }
        public string? Role { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }

        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }

        public string FullName => $"{LastName} {FirstName} {MiddleName}";
        public string? Sex { get; set; }
        public string? Photo { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Grade { get; set; }
        public string? ProgramType { get; set; }
        public string? Education { get; set; }
        public List<DateTime>? CertificationDates { get; set; }
        public bool HasBudoPassport { get; set; }
        public List<DateTime> PaymentDates { get; set; } = new();

        public string? City { get; set; }
        public string? ParentFullName { get; set; }
        public string? ParentPhoneNumber { get; set; }
        public DateTime? RegistrationDate { get; set; }

        public List<long>? UserMembershipIds { get; set; } = new();
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
            MiddleName = user.SecondName;
            Sex = user.Sex.ToString();
            Photo = user.Photo?.Length > 0 ? Convert.ToBase64String(user.Photo) : null;
            PhoneNumber = user.PhoneNumber;
            Birthday = user.Birthday;
            Grade = user.Grade.ToString();
            ProgramType = user.ProgramType.ToString();
            Education = user.Education.ToString();
            CertificationDates = user.CertificationDates?.ToList();
            HasBudoPassport = user.HasBudoPassport;
            PaymentDates = user.PaymentDates?.ToList() ?? new List<DateTime>();
            City = user.City;
            ParentFullName = user.ParentFullName;
            ParentPhoneNumber = user.ParentPhoneNumber;
            RegistrationDate = user.RegistrationDate;

        }

        public UserDto(UserEntity user, List<UserMembershipEntity> userMemberships) : this(user)
        {

            UserMembershipIds = userMemberships.Select(um => um.Id).ToList();
            UserMembershipDtos = userMemberships.Select(um =>  new UserMembershipDto(um)).ToList();
        }
    }
}