using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class GroupEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public long UserId { get; set; }
        public long ClubId { get; set; }
        public string? Name { get; set; }
        public string? AgeGroup { get; set; }
    }
}