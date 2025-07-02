using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class UserEntity : IDbEntity
    {

        public UserEntity()
        {

        }

        public UserEntity(Role role, string fullName)
        {
            Role = role;
            FullName = fullName;
        }

        [Key]
        public long Id { get; set; }

        public Role Role { get; set; }

        public string? Login { get; set; }
        public string? Password { get; set; }
        public string FullName { get; set; }

        public byte[] Photo { get; set; } = [];
        public string? PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public string? City { get; set; }
        public Grade? Grade { get; set; }
        public DateTime? CertificationDate { get; set; }
        public int? AnnualFee { get; set; }
        public Sex? Sex { get; set; }
        public long? GroupId { get; set; }
        public string? ParentFullName { get; set; }
        public string? ParentFullNumber { get; set; }
        public DateTime? RegistrationDate { get; set; }

        public void UpdateFromJson(UserJson userNewData)
        {
            if (userNewData.Role != null)
                Role = Enum.Parse<Role>(userNewData.Role);

            if (!string.IsNullOrEmpty(userNewData.Login))
                Login = userNewData.Login;

            if (!string.IsNullOrEmpty(userNewData.Password))
                Password = userNewData.Password;

            if (!string.IsNullOrEmpty(userNewData.FullName))
                FullName = userNewData.FullName;

            if (!string.IsNullOrEmpty(userNewData.Photo))
                Photo = Convert.FromBase64String(userNewData.Photo);

            if (!string.IsNullOrEmpty(userNewData.PhoneNumber))
                PhoneNumber = userNewData.PhoneNumber;

            if (userNewData.Birthday != null)
                Birthday = DateTime.SpecifyKind(userNewData.Birthday.Value, DateTimeKind.Utc);

            if (!string.IsNullOrEmpty(userNewData.City))
                City = userNewData.City;

            if (userNewData.Grade != null)
                Grade = Enum.Parse<Grade>(userNewData.Grade);

            if (userNewData.CertificationDate != null)
                CertificationDate = DateTime.SpecifyKind(userNewData.CertificationDate.Value, DateTimeKind.Utc);

            if (userNewData.AnnualFee != null)
                AnnualFee = userNewData.AnnualFee;

            if (userNewData.Sex != null)
                Sex = Enum.Parse<Sex>(userNewData.Sex);

            if (userNewData.GroupId != null)
                GroupId = userNewData.GroupId;

            if (!string.IsNullOrEmpty(userNewData.ParentFullName))
                ParentFullName = userNewData.ParentFullName;

            if (!string.IsNullOrEmpty(userNewData.ParentFullNumber))
                ParentFullNumber = userNewData.ParentFullNumber;

            if (userNewData.RegistrationDate != null)
                RegistrationDate = DateTime.SpecifyKind(userNewData.RegistrationDate.Value, DateTimeKind.Utc);
        }


    }
}
