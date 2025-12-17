using Aikido.Entities;
using Aikido.Entities.Seminar;
using Aikido.Entities.Seminar.SeminarMemberRequest;
using Aikido.Entities.Users;

namespace Aikido.Dto.Seminars.Members.CoachEditRequest
{
    public class SeminarMemberCoachRequestDto : DtoBase
    {
        public long SeminarId { get; set; }
        public string? SeminarName { get; set; }

        public long ClubId { get; set; }
        public string? ClubName { get; set; }

        public long RequestedById { get; set; }
        public string? RequestedByName { get; set; }
        public DateTime? CreatedAt { get; set; }

        public long? ReviewedById { get; set; }
        public string? ReviewedByName { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public string Status { get; set; }

        public string? ReviewerComment { get; set; }

        public SeminarMemberCoachRequestDto(SeminarMemberCoachRequestEntity request)
        {
            Id = request.Id;
            SeminarId = request.SeminarId;
            SeminarName = request.Seminar?.Name;
            ClubId = request.ClubId;
            ClubName = request.Club?.Name;
            RequestedById = request.RequestedById;
            RequestedByName = request.RequestedBy?.FullName;
            CreatedAt = request.CreatedAt;
            ReviewedById = request.ReviewedById;
            ReviewedByName = request.ReviewedBy?.FullName;
            ReviewedAt = request.ReviewedAt;

            Status = EnumParser.ConvertEnumToString(request.Status);
            ReviewerComment = request.ReviewerComment;
        }
    }
}
