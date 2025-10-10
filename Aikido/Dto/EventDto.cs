using Aikido.Entities;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Dto
{
    public class EventDto : DtoBase
    {
        public long? Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Location { get; set; }
        public string? EventType { get; set; }
        public long? GroupId { get; set; }
        public string? GroupName { get; set; }
        public long? ClubId { get; set; }
        public string? ClubName { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurrencePattern { get; set; }
        public bool IsActive { get; set; } = true;
        public int MaxParticipants { get; set; }
        public int CurrentParticipants { get; set; }

        public EventDto() { }

        public EventDto(EventEntity eventEntity)
        {
            Id = eventEntity.Id;
            Name = eventEntity.Name;
            Description = eventEntity.Description;
            StartDate = eventEntity.StartDate;
            EndDate = eventEntity.EndDate;
            Location = eventEntity.Location;
            EventType = eventEntity.EventType?.ToString();
            GroupId = eventEntity.GroupId;
            GroupName = eventEntity.Group?.Name;
            ClubId = eventEntity.ClubId;
            ClubName = eventEntity.Club?.Name;
            IsRecurring = eventEntity.IsRecurring;
            RecurrencePattern = eventEntity.RecurrencePattern;
            IsActive = eventEntity.IsActive;
            MaxParticipants = eventEntity.MaxParticipants;
            CurrentParticipants = eventEntity.CurrentParticipants;
        }
    }
}