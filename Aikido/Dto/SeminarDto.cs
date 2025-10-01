using Aikido.Entities.Seminar;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Dto.Seminars
{
    public class SeminarDto : DtoBase
    {
        public long? Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Location { get; set; }
        public string? Address { get; set; }
        public decimal? Cost { get; set; }
        public int MaxParticipants { get; set; }
        public int CurrentParticipants { get; set; }
        public string? InstructorName { get; set; }
        public long? InstructorId { get; set; }
        public string? Requirements { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? RegistrationDeadline { get; set; }
        public string? ContactInfo { get; set; }
        public List<string> Materials { get; set; } = new();

        public SeminarDto() { }

        public SeminarDto(SeminarEntity seminar)
        {
            Id = seminar.Id;
            Name = seminar.Name;
            Description = seminar.Description;
            StartDate = seminar.StartDate;
            EndDate = seminar.EndDate;
            Location = seminar.Location;
            Address = seminar.Address;
            Cost = seminar.Cost;
            MaxParticipants = seminar.MaxParticipants;
            CurrentParticipants = seminar.CurrentParticipants;
            InstructorName = seminar.InstructorName;
            InstructorId = seminar.InstructorId;
            Requirements = seminar.Requirements;
            IsActive = seminar.IsActive;
            RegistrationDeadline = seminar.RegistrationDeadline;
            ContactInfo = seminar.ContactInfo;
            Materials = seminar.Materials?.ToList() ?? new List<string>();
        }
    }
}