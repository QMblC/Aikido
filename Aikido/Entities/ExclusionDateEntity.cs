using Aikido.AdditionalData;
using Aikido.Dto.ExclusionDates;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class ExclusionDateEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public ExclusiveDateType Type { get; set; }
        public string? Description { get; set; }

        public long? GroupId { get; set; }
        public virtual GroupEntity? Group { get; set; }

        public ExclusionDateEntity() { }

        public ExclusionDateEntity(long groupId, IExclusionDateDto exclusionData)
        {
            UpdateFromJson(groupId, exclusionData);
        }

        public void UpdateFromJson(long groupId, IExclusionDateDto exclusionData)
        {
            Date = exclusionData.Date.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(exclusionData.Date, DateTimeKind.Utc)
                : exclusionData.Date.ToUniversalTime();
            Type = EnumParser.ConvertStringToEnum<ExclusiveDateType>(exclusionData.Type);
            Description = exclusionData.Description;
            GroupId = groupId;
            StartTime = exclusionData.StartTime;
            EndTime = exclusionData.EndTime;
        }

    }
}