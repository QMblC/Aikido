using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class StatisticsEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public int Year { get; set; }

        public int Month { get; set; }

        public int UsersCountStart { get; set; }

        public int UsersCountEnd { get; set; }

        public int MastersCountStart { get; set; }

        public int MastersCountEnd { get; set; }
    }
}
