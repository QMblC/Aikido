using Aikido.Dto;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class GroupEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public long CreatorId { get; set; }
        public List<long>? UserIds { get; set; }
        public long ClubId { get; set; }
        public string? Name { get; set; }
        public int AgeGroup { get; set; }


        public void UpdateFromJson(GroupDto groupNewData)
        {
            if (groupNewData.CreatorId != 0)
                CreatorId = groupNewData.CreatorId;

            if (groupNewData.UserIds != null)
            {
                UserIds = new();
                foreach (var item in groupNewData.UserIds)
                    UserIds.Append(item);
            }

            if (groupNewData.ClubId != 0)
                ClubId = groupNewData.ClubId;

            if (groupNewData.Name != null)
                Name = groupNewData.Name;

            if (groupNewData.AgeGroup != 0)
                AgeGroup = (int)groupNewData.AgeGroup;
        }
    }
}