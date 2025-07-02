using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class ClubEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public string Name { get; set; }

        public string City { get; set; }

        public string Address { get; set; }
    }
}
