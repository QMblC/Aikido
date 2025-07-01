using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public interface IDbEntity
    {
        [Key]
        public long id { get; set; }
    }
}
