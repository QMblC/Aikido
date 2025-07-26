using Aikido.AdditionalData;
using Aikido.Entities;

namespace Aikido.Dto
{
    public class GroupInfoDto : DtoBase
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
        public string? AgeGroup { get; set; }
        public long? CoachId { get; set; }
        public string? Coach { get; set; }
        public long? ClubId { get; set; }
        public string? Club { get; set; }
        public Dictionary<string, string>? Schedule { get; set; }
        public List<DateTime>? ExtraDates { get; set; }
        public List<DateTime>? MinorDates { get; set; }
        public List<UserShortDto>? GroupMembers { get; set; }

    }
}
