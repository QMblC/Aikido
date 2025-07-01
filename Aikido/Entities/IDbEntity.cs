using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public interface IDbEntity
    {
        [Key]
        public long Id { get; set; }
    }
}
