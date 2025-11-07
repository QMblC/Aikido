using Aikido.Services.DatabaseServices;
using Aikido.Exceptions;
using Aikido.Services;
using Aikido.Dto.Attendance;
using Aikido.Services.DatabaseServices.User;

namespace Aikido.Application.Services
{
    public class AttendanceApplicationService
    {
        private readonly AttendanceDbService _attendanceDbService;
        private readonly IUserDbService _userDbService;

        public AttendanceApplicationService(AttendanceDbService attendanceService, IUserDbService userDbService)
        {
            _attendanceDbService = attendanceService;
            _userDbService = userDbService;
        }

        public async Task<AttendanceDto> GetAttendanceByIdAsync(long id)
        {
            var attendance = await _attendanceDbService.GetAttendanceById(id);
            return new AttendanceDto(attendance);
        }

        public async Task<List<AttendanceDto>> GetAttendanceByGroup(long groupId, DateTime date)
        {
            var attendances = await _attendanceDbService.GetAttendancesByGroup(groupId, date);
            return attendances
                .Select(a => new AttendanceDto(a))
                .ToList();
        }

        public async Task<List<AttendanceDto>> GetAttendanceByUserAsync(long userId)
        {
            var attendances = await _attendanceDbService.GetAttendanceByUser(userId);
            return attendances.Select(a => new AttendanceDto(a)).ToList();
        }

        public async Task<long> CreateAttendanceAsync(long groupId, AttendanceCreationDto attendanceData)
        {
            var userMembership = _userDbService.GetUserMembership(attendanceData.UserId, groupId);
            return await _attendanceDbService.CreateAttendance(userMembership, attendanceData.Date);
        }

        public async Task DeleteAttendanceAsync(long id)
        {
            if (!await _attendanceDbService.AttendanceExists(id))
            {
                throw new EntityNotFoundException($"Посещаемость с Id = {id} не найдена");
            }
            await _attendanceDbService.DeleteAttendance(id);
        }

        public async Task<bool> AttendanceExistsAsync(long id)
        {
            return await _attendanceDbService.AttendanceExists(id);
        }
    }
}