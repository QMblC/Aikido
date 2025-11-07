using Aikido.Dto;
using Aikido.Dto.Attendance;
using Aikido.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class AttendanceEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long UserMembershipId { get; set; }
        public virtual UserMembershipEntity UserMembership { get; set; }

        public DateTime Date { get; set; }

        public AttendanceEntity() { }

        public AttendanceEntity(UserMembershipEntity userMembership, DateTime date)
        {
            UserMembership = userMembership;
            Date = date;
        }
    }
}