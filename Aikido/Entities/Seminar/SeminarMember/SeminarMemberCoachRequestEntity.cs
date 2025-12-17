using Aikido.AdditionalData.Enums;
using Aikido.Dto.Seminars.Members.CoachEditRequest;
using Aikido.Dto.Seminars.Members.Creation;
using Aikido.Entities.Seminar.SeminarMember;
using Aikido.Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Aikido.Entities.Seminar.SeminarMemberRequest
{
    public class SeminarMemberCoachRequestEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long SeminarId { get; set; }
        public virtual SeminarEntity? Seminar { get; set; }

        public long ClubId { get; set; }
        public virtual ClubEntity? Club { get; set; }

        public long RequestedById { get; set; }
        public virtual UserEntity? RequestedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public long? ReviewedById { get; set; }
        public virtual UserEntity? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public string? RequestJson { get; set; }

        public string? ReviewerComment { get; set; }

        public SeminarMemberCoachRequestEntity()
        {

        }

        public SeminarMemberCoachRequestEntity(long seminarId, SeminarMemberCoachRequestListCreationDto request)
        {
            SeminarId = seminarId;
            ClubId = request.ClubId.Value;
            RequestedById = request.CoachId.Value;
            RequestJson = JsonSerializer.Serialize(request.Members);
        }

        public void UpdateByCoach(SeminarMemberCoachRequestListCreationDto request)
        {
            Status = RequestStatus.Pending;
            RequestJson = JsonSerializer.Serialize(request.Members);
        }
    }
}
