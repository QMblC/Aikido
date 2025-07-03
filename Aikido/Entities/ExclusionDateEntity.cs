using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class ExclusionDateEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public long GroupId { get; set; }
        public DateTime Date { get; set; }
        public string? Status { get; set; }
    }
}