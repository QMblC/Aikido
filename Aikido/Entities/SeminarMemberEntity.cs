using Aikido.AdditionalData;
using Aikido.Dto.Seminars;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class SeminarMemberEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long UserId { get; set; }
        public long SeminarId { get; set; }
        public long ClubId { get; set; }
        public long GroupId { get; set; }
        public string? SeminarGroup { get; set; }
        public long CertificationId { get; set; }
        public List<long> PaymentsIds { get; set; } = new();

        public SeminarMemberEntity() { }
    }
}