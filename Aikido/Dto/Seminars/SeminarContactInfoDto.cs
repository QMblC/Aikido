using Aikido.Entities.Seminar;

namespace Aikido.Dto.Seminars
{
    public class SeminarContactInfoDto : DtoBase
    {
        public long? Id { get; set; }

        public string Description { get; set; }
        public string Value { get; set; }

        public SeminarContactInfoDto() { }

        public SeminarContactInfoDto(SeminarContactInfoEntity seminarContactInfo)
        {
            Description = seminarContactInfo.Description;
            Value = seminarContactInfo.Value;
        }
    }
}
