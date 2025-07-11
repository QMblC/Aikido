using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class SeminarMemberEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public long UserId { get; set; }
        public string GradeAttestation { get; set; }
        public bool GradeConfirmationStatus { get; set; } = false;
    }
}