using Aikido.AdditionalData;
using Aikido.Dto;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aikido.Entities.Users
{
    public class UserEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public Role Role { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }

        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public Sex Sex { get; set; }

        public string? AvatarPath { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public string? City { get; set; }

        public Grade Grade { get; set; } = Grade.None;
        public ProgramType ProgramType { get; set; } = ProgramType.None;
        public Education Education { get; set; } = Education.None;
        public bool HasBudoPassport { get; set; }

        public List<CertificationEntity> Certifications { get; set; } = [];
        public List<PaymentEntity> Payments { get; set; } = [];
        public List<UserGroupDataEntity> UserGroupData { get; set; } = [];

        public List<AttendanceEntity> Attendances { get; set; } = [];
        
        public string? ParentFullName { get; set; }
        public string? ParentPhoneNumber { get; set; }

        public DateTime? RegistrationDate { get; set; }

        [NotMapped]
        public string FullName => $"{LastName} {FirstName} {SecondName}";
        

        public UserEntity()
        {
        }

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
                    AvatarPath = Convert.FromBase64String(userNewData.Photo);
                }
                catch (FormatException)
                {
                    AvatarPath = [];
                }
            }
            else
            {
                AvatarPath = [];
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
            ClubId = userNewData.ClubId;
            GroupId = userNewData.GroupId;

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
