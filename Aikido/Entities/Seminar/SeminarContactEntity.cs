using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Seminar
{
    public class SeminarContactEntity
    {
        [Key]
        public long Id { get; set; }

        public long SeminarId { get; set; }
        public SeminarEntity Seminar { get; set; }

        public string Description { get; set; }
        public string Value { get; set; }
    }
}
