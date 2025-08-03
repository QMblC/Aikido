using Aikido.Dto;
using Aikido.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class AttendanceEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long UserId { get; set; }
        public UserEntity User { get; set; }

        public long GroupId { get; set; }
        public GroupEntity Group { get; set; }

        public DateTime VisitDate { get; set; }

        public async Task UpdateFromJson(AttendanceDto attendanceDto)
        {
            if (attendanceDto.UserId != null)
            {
                UserId = attendanceDto.UserId;
            }

            if (attendanceDto.GroupId != null)
            {
                GroupId = attendanceDto.GroupId;
            }

            if (attendanceDto.VisitDate != null)
            {
                VisitDate = DateTime.SpecifyKind(attendanceDto.VisitDate.Value, DateTimeKind.Utc);
            }
        }
    
    }


}
