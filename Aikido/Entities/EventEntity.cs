using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class EventEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
    }
}