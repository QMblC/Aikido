using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class StudentEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public long GroupId { get; set; }
    }
}