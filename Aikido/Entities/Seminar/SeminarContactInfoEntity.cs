using Aikido.Dto.Seminars;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Seminar
{
    public class SeminarContactInfoEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        
        public long SeminarId { get; set; }
        public virtual SeminarEntity? Seminar { get; set; }

        public string Name { get; set; }
        public string? FirstContact { get; set; }
        public string? SecondContact { get; set; }
        public string Description { get; set; }

        public SeminarContactInfoEntity() { }

        public SeminarContactInfoEntity(long seminarId, ISeminarContactInfoDto seminarContactInfoDto)
        {
            SeminarId = seminarId;
            Name = seminarContactInfoDto.Name;
            FirstContact = seminarContactInfoDto.FirstContact;
            SecondContact = seminarContactInfoDto.SecondContact;
            Description = seminarContactInfoDto.Description;
        }
    }
}
