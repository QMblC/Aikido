using Aikido.AdditionalData;
using Aikido.Dto.Seminars;
using Aikido.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Seminar
{
    public class SeminarMemberEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long UserId { get; set; }
        public UserEntity User { get; set; }

        public long SeminarId { get; set; }
        public SeminarEntity Seminar { get; set; }

        public long ClubId { get; set; }
        public ClubEntity Club { get; set; }

        public long SeminarGroupId { get; set; }
        public SeminarGroupEntity SeminarGroup { get; set; }

        public long? CertificationId { get; set; }
        public CertificationEntity? Certification { get; set; }

        public List<PaymentEntity> Payments { get; set; } = new();

        public SeminarMemberEntity() { }
    }
}