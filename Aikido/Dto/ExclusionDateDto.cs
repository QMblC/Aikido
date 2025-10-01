using Aikido.Entities;

namespace Aikido.Dto
{
    public class ExclusionDateDto : DtoBase
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long? GroupId { get; set; }
        public string? GroupName { get; set; }
        public long? ClubId { get; set; }
        public string? ClubName { get; set; }
        public bool IsRecurring { get; set; }
        public DateTime? RecurringUntil { get; set; }
        public string? RecurringPattern { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedDate { get; set; }
        public long? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }

        public ExclusionDateDto() { }

        public ExclusionDateDto(ExclusionDateEntity exclusionDate)
        {
            Id = exclusionDate.Id;
            Date = exclusionDate.Date;
            Type = exclusionDate.Type.ToString();
            Description = exclusionDate.Description;
            GroupId = exclusionDate.GroupId;
            ClubId = exclusionDate.ClubId;
            IsRecurring = exclusionDate.IsRecurring;
            RecurringUntil = exclusionDate.RecurringUntil;
            RecurringPattern = exclusionDate.RecurringPattern;
            IsActive = exclusionDate.IsActive;
            CreatedDate = exclusionDate.CreatedDate;
            CreatedBy = exclusionDate.CreatedBy;
        }
    }
}