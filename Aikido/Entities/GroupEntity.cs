using Aikido.Dto;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class GroupEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public long CoachId { get; set; }
        public List<long>? UserIds { get; set; }
        public long ClubId { get; set; }
        public string? Name { get; set; }
        public string? AgeGroup { get; set; }

        public void UpdateFromJson(GroupDto groupNewData)
        {
            if (groupNewData.CoachId != null)
                CoachId = (long)groupNewData.CoachId;

            if (groupNewData.UserIds != null)
                UserIds = new (groupNewData.UserIds);

            if (groupNewData.ClubId != null)
                ClubId = (long)groupNewData.ClubId;

            if (!string.IsNullOrEmpty(groupNewData.Name))
                Name = groupNewData.Name;

            if (!string.IsNullOrEmpty(groupNewData.AgeGroup))
                AgeGroup = groupNewData.AgeGroup;
        }

    }
}