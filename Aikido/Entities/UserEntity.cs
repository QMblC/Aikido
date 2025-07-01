using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class UserEntity
    {
        [Key]
        public long Id { get; set; }
    }
}
