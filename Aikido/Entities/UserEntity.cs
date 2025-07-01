using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class UserEntity : IDbEntity
    {
        [Key]
        public long id { get; set; }

        public Role role { get; set; }

        [Required]
        public string login { get; set; }
        [Required]
        public string password { get; set; }
        [Required]
        public string fullName { get; set; }

        public byte[] photo { get; set; } = [];
        public string phoneNumber { get; set; }
        public DateTime birthday { get; set; }
        public string city { get; set; }
        public Grade grade { get; set; }
        public DateTime certificationDate { get; set; }
        public int annualFee { get; set; }
        public Sex sex { get; set; }
        public long groupId { get; set; }
        public string parentFullName { get; set; }
        public string parentFullNumber { get; set; }
        public DateTime registrationDate { get; set; }
    }
}
