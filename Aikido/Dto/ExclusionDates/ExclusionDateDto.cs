using Aikido.Entities;

namespace Aikido.Dto.ExclusionDates
{
    public class ExclusionDateDto : DtoBase, IExclusionDateDto
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long? GroupId { get; set; }
        public string? GroupName { get; set; }

        public ExclusionDateDto() { }

        public ExclusionDateDto(ExclusionDateEntity exclusionDate)
        {
            Id = exclusionDate.Id;
            Date = exclusionDate.Date;
            Type = exclusionDate.Type.ToString();
            Description = exclusionDate.Description;
            GroupId = exclusionDate.GroupId;
            StartTime = exclusionDate.StartTime;
            EndTime = exclusionDate.EndTime;
        }
    }
}