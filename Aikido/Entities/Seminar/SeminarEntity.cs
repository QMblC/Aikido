using Aikido.Dto;
using Aikido.Dto.Seminars;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Seminar
{
    public class SeminarEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
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
        public virtual UserEntity? Instructor { get; set; }
        public string? Requirements { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? RegistrationDeadline { get; set; }
        public string? ContactInfo { get; set; }
        public List<string> Materials { get; set; } = new();
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        // Навигационные свойства
        public virtual ICollection<SeminarMemberEntity> SeminarMembers { get; set; } = new List<SeminarMemberEntity>();

        public SeminarEntity() { }

        public SeminarEntity(SeminarDto seminarData)
        {
            UpdateFromJson(seminarData);
        }

        public void UpdateFromJson(SeminarDto seminarData)
        {
            if (!string.IsNullOrEmpty(seminarData.Name))
                Name = seminarData.Name;
            Description = seminarData.Description;
            StartDate = seminarData.StartDate;
            EndDate = seminarData.EndDate;
            Location = seminarData.Location;
            Address = seminarData.Address;
            Cost = seminarData.Cost;
            MaxParticipants = seminarData.MaxParticipants;
            CurrentParticipants = seminarData.CurrentParticipants;
            InstructorName = seminarData.InstructorName;
            InstructorId = seminarData.InstructorId;
            Requirements = seminarData.Requirements;
            IsActive = seminarData.IsActive;
            RegistrationDeadline = seminarData.RegistrationDeadline;
            ContactInfo = seminarData.ContactInfo;
            Materials = seminarData.Materials?.ToList() ?? new List<string>();
            UpdatedDate = DateTime.UtcNow;
        }
    }
}