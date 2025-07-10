using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class UserEntity : IDbEntity
    {
        public UserEntity()
        {
        }

        public UserEntity(string role, string fullName)
        {
            Role = role;
            FullName = fullName;
        }

        [Key]
        public long Id { get; set; }

        public string? Role { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
        public string FullName { get; set; }

        public byte[] Photo { get; set; } = [];
        public string? PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public string? City { get; set; }
        public string? Grade { get; set; }
        public DateTime? CertificationDate { get; set; }
        public int? AnnualFee { get; set; }
        public string? Sex { get; set; }
        public long? ClubId { get; set; }
        public long? GroupId { get; set; }
        public int? SchoolClass { get; set; }
        public string? ParentFullName { get; set; }
        public string? ParentFullNumber { get; set; }
        public DateTime? RegistrationDate { get; set; }

        public void UpdateFromJson(UserDto userNewData)
        {
            if (userNewData.Id != null)
                Id = (long)userNewData.Id;

            if (userNewData.Role != null)
                Role = userNewData.Role;

            if (!string.IsNullOrEmpty(userNewData.Login))
                Login = userNewData.Login;

            if (!string.IsNullOrEmpty(userNewData.Password))
                Password = userNewData.Password;

            if (!string.IsNullOrEmpty(userNewData.FullName))
                FullName = userNewData.FullName;

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

            if (userNewData.Birthday != null)
                Birthday = DateTime.SpecifyKind(userNewData.Birthday.Value, DateTimeKind.Utc);
            else
                Birthday = null;

            City = userNewData.City;

            if (userNewData.Grade != null)
                Grade = userNewData.Grade;

            if (userNewData.CertificationDate != null)
                CertificationDate = DateTime.SpecifyKind(userNewData.CertificationDate.Value, DateTimeKind.Utc);
            else
                CertificationDate = null;

            AnnualFee = userNewData.AnnualFee ?? 0;

            if (userNewData.Sex != null)
                Sex = userNewData.Sex;

            SchoolClass = userNewData.SchoolClass;
            ClubId = userNewData.ClubId;
            GroupId = userNewData.GroupId;

            ParentFullName = userNewData.ParentFullName;
            ParentFullNumber = userNewData.ParentFullNumber;

            if (userNewData.RegistrationDate != null)
                RegistrationDate = DateTime.SpecifyKind(userNewData.RegistrationDate.Value, DateTimeKind.Utc);
            else
                RegistrationDate = null;
        }
    }
}
