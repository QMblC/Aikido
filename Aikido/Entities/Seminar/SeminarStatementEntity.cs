using Aikido.Dto;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Seminar
{
    public class SeminarStatementEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public string? Name { get; set; }

        public long SeminarId { get; set; }
        public virtual SeminarEntity Seminar { get; set; }

        public byte[]? File { get; set; }


        public SeminarStatementEntity() { }

    }
}
