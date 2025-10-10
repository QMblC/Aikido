using Aikido.AdditionalData;
using Aikido.Dto;
using DocumentFormat.OpenXml.Features;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class EventEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Location { get; set; }
        public EventType? EventType { get; set; }

        public long? GroupId { get; set; }
        public virtual GroupEntity? Group { get; set; }

        public long? ClubId { get; set; }
        public virtual ClubEntity? Club { get; set; }

        public bool IsRecurring { get; set; }
        public string? RecurrencePattern { get; set; }
        public bool IsActive { get; set; } = true;
        public int MaxParticipants { get; set; }
        public int CurrentParticipants { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        // Навигационные свойства
        public virtual ICollection<AttendanceEntity> Attendances { get; set; } = new List<AttendanceEntity>();

        public EventEntity() { }

        public EventEntity(EventDto eventData)
        {
            UpdateFromJson(eventData);
        }

        public void UpdateFromJson(EventDto eventData)
        {
            if (!string.IsNullOrEmpty(eventData.Name))
                Name = eventData.Name;
            Description = eventData.Description;
            StartDate = eventData.StartDate;
            EndDate = eventData.EndDate;
            Location = eventData.Location;
            if (!string.IsNullOrEmpty(eventData.EventType))
                EventType = EnumParser.ConvertStringToEnum<EventType>(eventData.EventType);
            GroupId = eventData.GroupId;
            ClubId = eventData.ClubId;
            IsRecurring = eventData.IsRecurring;
            RecurrencePattern = eventData.RecurrencePattern;
            IsActive = eventData.IsActive;
            MaxParticipants = eventData.MaxParticipants;
            CurrentParticipants = eventData.CurrentParticipants;
            UpdatedDate = DateTime.UtcNow;
        }
    }
}