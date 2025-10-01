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
        public ExclusiveDateType Type { get; set; }
        public string? Description { get; set; }

        public long? GroupId { get; set; }
        public virtual GroupEntity? Group { get; set; }

        public long? ClubId { get; set; }
        public virtual ClubEntity? Club { get; set; }

        public bool IsRecurring { get; set; }
        public DateTime? RecurringUntil { get; set; }
        public string? RecurringPattern { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;

        public long? CreatedBy { get; set; }
        public virtual UserEntity? CreatedByUser { get; set; }

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
            ClubId = exclusionData.ClubId;
            IsRecurring = exclusionData.IsRecurring;
            RecurringUntil = exclusionData.RecurringUntil;
            RecurringPattern = exclusionData.RecurringPattern;
            IsActive = exclusionData.IsActive;
            CreatedBy = exclusionData.CreatedBy;
        }
    }
}