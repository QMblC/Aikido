using Aikido.AdditionalData.Enums;
using Aikido.Dto.FormerCertifications;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class FormerCertificationEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long UserId { get; set; }
        public virtual UserEntity? User { get; set; }
        public DateTime Date { get; set; }

        public Grade CertificationGrade { get; set; }

        public FormerCertificationEntity() { }

        public FormerCertificationEntity(long userId, IFormerCertificationCreationDto certification)
        {
            UserId = userId;
            Date = certification.Date;
            CertificationGrade = EnumParser.ConvertStringToEnum<Grade>(certification.CertificationGrade);
        }
    }
}
