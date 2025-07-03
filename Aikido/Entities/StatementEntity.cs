using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class StatementEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public long SeminarId { get; set; }
        public long UserId { get; set; }
        public byte[]? StatementFile { get; set; }
    }
}