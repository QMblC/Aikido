using Aikido.AdditionalData;
using Aikido.Dto;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class ExclusionDateEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public long GroupId { get; set; }
        public DateTime Date { get; set; }
        //public TimeSpan StartTime { get; set; }
        //public TimeSpan EndTime { get; set; }
        public ExclusiveDateType Status { get; set; }

        public ExclusionDateEntity() { }

        public ExclusionDateEntity(ExclusionDateDto exclusionDate)
        {
            if (exclusionDate.GroupId != null)
            {
                GroupId = exclusionDate.GroupId.Value;
            }
            if (exclusionDate.Date != null)
            {
                Date = exclusionDate.Date;
            }
            if (exclusionDate.Status != null)
            {
                Status = EnumParser.ConvertStringToEnum<ExclusiveDateType>(exclusionDate.Status);
            }
        }
    }
}