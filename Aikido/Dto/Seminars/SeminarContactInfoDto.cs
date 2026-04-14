using Aikido.Entities.Seminar;

namespace Aikido.Dto.Seminars
{
    public class SeminarContactInfoDto : DtoBase, ISeminarContactInfoDto
    {
        public long? Id { get; set; }

        public string Name { get; set; }
        public string FirstContact { get; set; }
        public string? SecondContact { get; set; }
        public string Description { get; set; } = "";
        

        public SeminarContactInfoDto() { }

        public SeminarContactInfoDto(SeminarContactInfoEntity seminarContactInfo)
        {
            Id = seminarContactInfo.Id;
            Name = seminarContactInfo.Name;
            FirstContact = seminarContactInfo.FirstContact;
            SecondContact = seminarContactInfo.SecondContact;
            Description = seminarContactInfo.Description;    
        }
    }
}
