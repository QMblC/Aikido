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

        public string Description { get; set; }
        public string Value { get; set; }

        public SeminarContactInfoEntity(SeminarEntity seminarEntity, SeminarContactInfoDto seminarContactInfoDto)
        {
            SeminarId = seminarEntity.Id;
            Description = seminarContactInfoDto.Description;
            Value = seminarContactInfoDto.Value;
        }

        public SeminarContactInfoEntity(long seminarId, string description, string value)
        {
            UpdateFromJson(seminarId, description, value);
        }

        public void UpdateFromJson(long seminarId, string description, string value)
        {
            SeminarId = seminarId;
            Description = description;
            Value = value;
        }
    }
}
