using Aikido.Entities;
using Aikido.Exceptions;

namespace Aikido.Services.DatabaseServices.Attendance
{
    public interface IAttendanceDbService
    {
        Task<AttendanceEntity> GetAttendanceById(long id);
        Task<List<AttendanceEntity>> GetAttendancesByGroup(long groupId, DateTime date);
        Task<List<AttendanceEntity>> GetAttendanceByUser(long userId);
        Task<long> CreateAttendance(long userMembershipId, DateTime date);
        Task CreateAttendances(Dictionary<long, List<DateTime>> data);
        Task DeleteAttendances(List<long> idsToDelete);
        Task DeleteAttendance(long id);
        Task<bool> AttendanceExists(long id);
        Task<bool> AttendanceExists(long userId, DateTime date);
    }
}
