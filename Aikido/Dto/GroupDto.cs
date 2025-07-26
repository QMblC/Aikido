using Aikido.AdditionalData;
using Aikido.Entities;

namespace Aikido.Dto
{
    public class GroupDto : DtoBase
    {
        public long? CoachId { get; set; }
        public List<long>? GroupMembers { get; set; }
        public long? ClubId { get; set; }
        public string? Name { get; set; }
        public string? AgeGroup { get; set; }

        public GroupDto() { }

        public GroupDto(GroupEntity groupEntity)
        {
            Id = groupEntity.Id;
            CoachId = groupEntity.CoachId;
            GroupMembers = groupEntity.UserIds;
            ClubId = groupEntity.ClubId;
            Name = groupEntity.Name;
            AgeGroup = EnumParser.ConvertEnumToString(groupEntity.AgeGroup);
        }
    }

}
