using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class UserEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public Role Role { get; set; }

        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string FullName { get; set; }

        public byte[] Photo { get; set; } = [];
        public string PhoneNumber { get; set; }
        public DateTime Birthday { get; set; }
        public string City { get; set; }
        public Grade Grade { get; set; }
        public DateTime CertificationDate { get; set; }
        public int AnnualFee { get; set; }
        public Sex Sex { get; set; }
        public long GroupId { get; set; }
        public string ParentFullName { get; set; }
        public string ParentFullNumber { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}
