using Aikido.AdditionalData.Enums;
using Aikido.Dto.Attendance;
using Aikido.Exceptions;

namespace Aikido.Services.ApplicationServices.Attendnace
{
    public interface IAttendanceApplicationService
    {
        Task<AttendanceDto> GetAttendanceByIdAsync(long id);
        Task<List<AttendanceDto>> GetAttendanceByGroup(long groupId, DateTime date);
        Task<List<AttendanceDto>> GetAttendanceByUserAsync(long userId);
        Task<long> MarkAttendanceAsync(long groupId, AttendanceCreationDto attendanceData);
        Task UpdateAttendances(long groupId, AttendanceUpdateDto attendanceData);
        Task DeleteAttendanceAsync(long id);
        Task<bool> AttendanceExistsAsync(long id);
    }
}
