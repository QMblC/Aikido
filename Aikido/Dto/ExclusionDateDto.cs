using Aikido.AdditionalData;
using Aikido.Entities;

namespace Aikido.Dto
{
    public class ExclusionDateDto : DtoBase
    {
        public long? Id { get; set; }
        public long? GroupId { get; set; }
        public DateTime Date { get; set; }
        public string? Status { get; set; }

        public ExclusionDateDto() { }

        public ExclusionDateDto(ExclusionDateEntity exclusionDate)
        {
            Id = exclusionDate.Id;
            GroupId = exclusionDate.GroupId;
            Date = exclusionDate.Date;
            Status = EnumParser.ConvertEnumToString(exclusionDate.Status);
        }
    }
}
