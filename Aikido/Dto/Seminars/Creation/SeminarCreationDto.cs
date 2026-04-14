
namespace Aikido.Dto.Seminars.Creation
{
    public class SeminarCreationDto : ISeminarDto
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }

        public long? CreatorId { get; set; }
        public List<long> Editors { get; set; } = new();

        public List<SeminarContactInfoCreationDto>? ContactInfo { get; set; }
        public List<SeminarGroupCreationDto>? Groups { get; set; }
        public List<SeminarScheduleCreationDto>? Schedule { get; set; }
        public List<SeminarPriceCreationDto>? Prices { get; set; }
        
    }
}
