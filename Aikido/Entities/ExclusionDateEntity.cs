using Aikido.AdditionalData;
using Aikido.Dto;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class ExclusionDateEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public ExclusiveDateType Type { get; set; }
        public string? Description { get; set; }

        public long? GroupId { get; set; }
        public virtual GroupEntity? Group { get; set; }

        public ExclusionDateEntity() { }

        public ExclusionDateEntity(ExclusionDateDto exclusionData)
        {
            UpdateFromJson(exclusionData);
        }

        public void UpdateFromJson(ExclusionDateDto exclusionData)
        {
            Date = exclusionData.Date;
            Type = EnumParser.ConvertStringToEnum<ExclusiveDateType>(exclusionData.Type);
            Description = exclusionData.Description;
            GroupId = exclusionData.GroupId;
            StartTime = exclusionData.StartTime;
            EndTime = exclusionData.EndTime;
        }
    }
}