using Aikido.AdditionalData;
using Aikido.Dto;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class UserEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public Role Role { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
        public string FullName { get; set; } = string.Empty;
        public Sex Sex { get; set; }
        public byte[] Photo { get; set; } = [];
        public string? PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public Grade Grade { get; set; } = Grade.None;
        public ProgramType ProgramType { get; set; } = ProgramType.None;
        public Education Education { get; set; } = Education.None;
        public List<DateTime> CertificationDates { get; private set; } = [];
        public bool HasBudoPassport { get; set; }
        public List<DateTime> PaymentDates { get; set; } = [];

        // Удалены ClubId и GroupId - теперь связи через промежуточные таблицы
        public string? City { get; set; }

        public string? ParentFullName { get; set; }
        public string? ParentPhoneNumber { get; set; }
        public DateTime? RegistrationDate { get; set; }

        // Навигационные свойства для many-to-many связей
        public virtual ICollection<UserClub> UserClubs { get; set; } = new List<UserClub>();
        public virtual ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();

        public UserEntity() { }

        public UserEntity(string role, string fullName)
        {
            Role = EnumParser.ConvertStringToEnum<Role>(role);
            FullName = fullName;
        }

        public UserEntity(UserDto userNewData)
        {
            UpdateFromJson(userNewData);
        }

        public void UpdateFromJson(UserDto userNewData)
        {
            if (userNewData.Role != null)
                Role = EnumParser.ConvertStringToEnum<Role>(userNewData.Role);
            if (!string.IsNullOrEmpty(userNewData.Login))
                Login = userNewData.Login;
            if (!string.IsNullOrEmpty(userNewData.Password))
                Password = userNewData.Password;
            if (!string.IsNullOrEmpty(userNewData.Name))
                FullName = userNewData.Name;
            if (!string.IsNullOrEmpty(userNewData.Photo))
            {
                try
                {
                    Photo = Convert.FromBase64String(userNewData.Photo);
                }
                catch (FormatException)
                {
                    Photo = [];
                }
            }
            else
            {
                Photo = [];
            }
            PhoneNumber = userNewData.PhoneNumber;
            ProgramType = EnumParser.ConvertStringToEnum<ProgramType>(userNewData.ProgramType);
            if (userNewData.Birthday != null)
                Birthday = DateTime.SpecifyKind(userNewData.Birthday.Value, DateTimeKind.Utc);
            else
                Birthday = null;
            City = userNewData.City;
            Grade = EnumParser.ConvertStringToEnum<Grade>(userNewData.Grade);
            if (userNewData.CertificationDates != null)
                CertificationDates = userNewData.CertificationDates;
            else
                CertificationDates = [];
            if (userNewData.Sex != null)
                Sex = EnumParser.ConvertStringToEnum<Sex>(userNewData.Sex);
            Education = EnumParser.ConvertStringToEnum<Education>(userNewData.Education);
            ParentFullName = userNewData.ParentFullName;
            ParentPhoneNumber = userNewData.ParentPhoneNumber;
            if (userNewData.RegistrationDate != null)
                RegistrationDate = DateTime.SpecifyKind(userNewData.RegistrationDate.Value, DateTimeKind.Utc);
            else
                RegistrationDate = null;
        }

        public void AddCertificationDate(DateTime date)
        {
            CertificationDates.Add(date);
        }

        public void RemoveCertificationDate(DateTime date)
        {
            CertificationDates.Remove(date);
        }
    }
}