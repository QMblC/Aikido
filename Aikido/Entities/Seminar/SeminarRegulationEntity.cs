using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Seminar
{
    public class SeminarRegulationEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long SeminarId { get; set; }
        public virtual SeminarEntity? Seminar { get; set; }

        public byte[]? File { get; set; }

        public SeminarRegulationEntity(long seminarId, byte[] file)
        {
            SeminarId = seminarId;
            File = file;
        }
    }
}
