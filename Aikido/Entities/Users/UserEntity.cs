using Aikido.AdditionalData.Enums;
using Aikido.Dto.Users;
using Aikido.Dto.Users.Creation;
using Aikido.Entities.Seminar.SeminarMember;
using Aikido.Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aikido.Entities
{
    public class UserEntity : IDbEntity, IEquatable<UserEntity>
    {
        [Key]
        public long Id { get; set; }
        public Role Role { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }

        public Sex Sex { get; set; }
        public byte[] Photo { get; set; } = [];
        public string? PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public Grade Grade { get; set; } = Grade.None;
        public Education Education { get; set; } = Education.None;
        
        public bool HasBudoPassport { get; set; } = false;
        public List<DateTime> PaymentDates { get; set; } = [];

        public string? City { get; set; }

        public string? ParentFullName { get; set; }
        public string? ParentPhoneNumber { get; set; }

        public DateTime? RegistrationDate { get; set; }

        public long? CreatorId { get; set; }
        public virtual UserEntity Creator { get; set; }
        public virtual DateTime? CreationDate { get; set; }


        public virtual ICollection<SeminarMemberEntity> Certifications { get; set; } = new List<SeminarMemberEntity>();
        public virtual ICollection<PaymentEntity> Payments { get; set; } = new List<PaymentEntity>();
        public virtual ICollection<UserMembershipEntity> UserMemberships { get; set; } = new List<UserMembershipEntity>();
        public virtual ICollection<RefreshTokenEntity> RefreshTokens { get; set; } = new List<RefreshTokenEntity>();


        [NotMapped]
        public string FullName => $"{LastName} {FirstName} {MiddleName}";

        public UserEntity() { }

        public UserEntity(string role, string lastName, string firstName, string secondName)
        {
            Role = EnumParser.ConvertStringToEnum<Role>(role);
            LastName = lastName;
            FirstName = firstName;
            MiddleName = secondName;
        }

        public UserEntity(IUserDto userNewData)
        {
            UpdateFromJson(userNewData);
            CreationDate = DateTime.UtcNow;
        }

        public void UpdateFromJson(IUserDto userNewData)
        {
            if (userNewData.Role != null)
                Role = EnumParser.ConvertStringToEnum<Role>(userNewData.Role);
            if (!string.IsNullOrEmpty(userNewData.Login))
                Login = userNewData.Login;
            if (!string.IsNullOrEmpty(userNewData.Password))
                Password = BCrypt.Net.BCrypt.HashPassword(userNewData.Password);
            if (!string.IsNullOrEmpty(userNewData.LastName))
                LastName = userNewData.LastName;
            if (!string.IsNullOrEmpty(userNewData.FirstName))
                FirstName = userNewData.FirstName;
            if (!string.IsNullOrEmpty(userNewData.MiddleName))
                MiddleName = userNewData.MiddleName;
            if (userNewData.Photo != null)
            {
                Photo = userNewData.Photo;
            }
            else
            {
                Photo = [];
            }
            PhoneNumber = userNewData.PhoneNumber;
            if (userNewData.Birthday != null)
                Birthday = DateTime.SpecifyKind(userNewData.Birthday.Value, DateTimeKind.Utc);
            else
                Birthday = null;
            City = userNewData.City;
            Grade = EnumParser.ConvertStringToEnum<Grade>(userNewData.Grade);
            HasBudoPassport = userNewData.HasBudoPassport != null ? userNewData.HasBudoPassport.Value : false;
            if (userNewData.Sex != null)
                Sex = EnumParser.ConvertStringToEnum<Sex>(userNewData.Sex);
            Education = EnumParser.ConvertStringToEnum<Education>(userNewData.Education);
            ParentFullName = userNewData.ParentFullName;
            ParentPhoneNumber = userNewData.ParentPhoneNumber;

            if (userNewData.RegistrationDate != null)
                RegistrationDate = DateTime.SpecifyKind(userNewData.RegistrationDate.Value, DateTimeKind.Utc);
            else
                RegistrationDate = DateTime.UtcNow;
        }

        public bool Equals(UserEntity? other)
        {
            if (other == null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id == other.Id;
        }
    }
}