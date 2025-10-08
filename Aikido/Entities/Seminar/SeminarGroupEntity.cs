using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Seminar
{
    public class SeminarGroupEntity
    {
        [Key]
        public long Id { get; set; }

        public long SeminarId { get; set; }
        public SeminarEntity Seminar { get; set; }

        public string Name { get; set; }
    }
}
