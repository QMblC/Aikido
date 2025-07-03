using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class SeminarMemberEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public long UserId { get; set; }
        public int KyuAttestation { get; set; }
        public bool KyuConfirmationStatus { get; set; }
    }
}