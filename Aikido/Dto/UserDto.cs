using Aikido.AdditionalData;
using Aikido.Entities;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Dto
{
    public class UserDto : DtoBase
    {
        public long? Id { get; set; }
        public string? Role { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
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

        public List<long>? ClubIds { get; set; }
        public List<string>? ClubNames { get; set; }
        public List<long>? GroupIds { get; set; }
        public List<string>? GroupNames { get; set; }

        public string? City { get; set; }
        public string? ParentFullName { get; set; }
        public string? ParentPhoneNumber { get; set; }
        public DateTime? RegistrationDate { get; set; }

        public List<UserClubDto>? UserClubs { get; set; }
        public List<UserGroupDto>? UserGroups { get; set; }

        public UserDto() { }

        public UserDto(UserEntity user)
        {
            Id = user.Id;
            Role = user.Role.ToString();
            Login = user.Login;
            Password = user.Password;
            Name = user.FullName;
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

            ClubIds = new List<long>();
            ClubNames = new List<string>();
            GroupIds = new List<long>();
            GroupNames = new List<string>();
            UserClubs = new List<UserClubDto>();
            UserGroups = new List<UserGroupDto>();
        }

        public UserDto(UserEntity user, List<UserClub> userClubs, List<UserGroup> userGroups) : this(user)
        {
            ClubIds = userClubs.Where(uc => uc.IsActive).Select(uc => uc.ClubId).ToList();
            ClubNames = userClubs.Where(uc => uc.IsActive && uc.Club != null).Select(uc => uc.Club!.Name).ToList();
            UserClubs = userClubs.Select(uc => new UserClubDto(uc)).ToList();

            GroupIds = userGroups.Where(ug => ug.IsActive).Select(ug => ug.GroupId).ToList();
            GroupNames = userGroups.Where(ug => ug.IsActive && ug.Group != null).Select(ug => ug.Group!.Name).ToList();
            UserGroups = userGroups.Select(ug => new UserGroupDto(ug)).ToList();
        }
    }
}