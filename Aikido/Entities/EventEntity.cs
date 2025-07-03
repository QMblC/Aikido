using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class EventEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public long UserId { get; set; }
        public DateTime PublishDate { get; set; }
        public string? City { get; set; }
        public string? Title { get; set; }
        public DateTime EventDate { get; set; }
        public string? Description { get; set; }
        public byte[] File { get; set; } = [];
    }
}