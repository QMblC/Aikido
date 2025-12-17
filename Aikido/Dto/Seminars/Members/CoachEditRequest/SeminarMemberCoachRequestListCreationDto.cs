using Aikido.Dto.Seminars.Members.Creation;
using Aikido.Entities.Seminar.SeminarMemberRequest;
using System.Text.Json;

namespace Aikido.Dto.Seminars.Members.CoachEditRequest
{
    public class SeminarMemberCoachRequestListCreationDto
    {
        public long? CoachId { get; set; }
        public long? ClubId { get; set; }
        public List<SeminarMemberRequestCreationDto> Members { get; set; } = new();

        public SeminarMemberCoachRequestListCreationDto()
        {

        }

        public SeminarMemberCoachRequestListCreationDto(SeminarMemberCoachRequestEntity request)
        {
            CoachId = request.RequestedById;
            ClubId = request.ClubId;
            Members = JsonSerializer.Deserialize<List<SeminarMemberRequestCreationDto>>(request.RequestJson);
        }

    }
}
