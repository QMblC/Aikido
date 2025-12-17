using Aikido.AdditionalData.Enums;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class CertificationEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long UserId { get; set; }
        public UserEntity User { get; set; }
        
        public Grade OldGrade { get; set; }
        public Grade CertificationGrade { get; set; }
        public bool GradeConfirmationStatus { get; set; } = false;
        public SeminarMemberStatus ResultStatus { get; set; }

        public DateTime Date { get; set; }
    }
}
