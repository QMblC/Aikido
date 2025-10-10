using Aikido.Dto;
using Aikido.Services.DatabaseServices;
using Aikido.Exceptions;
using Aikido.Services;

namespace Aikido.Application.Services
{
    public class AttendanceApplicationService
    {
        private readonly AttendanceService _attendanceService;

        public AttendanceApplicationService(AttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        public async Task<AttendanceDto> GetAttendanceByIdAsync(long id)
        {
            var attendance = await _attendanceService.GetAttendanceById(id);
            return new AttendanceDto(attendance);
        }

        public async Task<List<AttendanceDto>> GetAttendanceByUserAsync(long userId)
        {
            var attendances = await _attendanceService.GetAttendanceByUser(userId);
            return attendances.Select(a => new AttendanceDto(a)).ToList();
        }

        public async Task<List<AttendanceDto>> GetAttendanceByEventAsync(long eventId)
        {
            var attendances = await _attendanceService.GetAttendanceByEvent(eventId);
            return attendances.Select(a => new AttendanceDto(a)).ToList();
        }

        public async Task<long> CreateAttendanceAsync(AttendanceDto attendanceData)
        {
            return await _attendanceService.CreateAttendance(attendanceData);
        }

        public async Task UpdateAttendanceAsync(long id, AttendanceDto attendanceData)
        {
            if (!await _attendanceService.AttendanceExists(id))
            {
                throw new EntityNotFoundException($"Посещаемость с Id = {id} не найдена");
            }
            await _attendanceService.UpdateAttendance(id, attendanceData);
        }

        public async Task DeleteAttendanceAsync(long id)
        {
            if (!await _attendanceService.AttendanceExists(id))
            {
                throw new EntityNotFoundException($"Посещаемость с Id = {id} не найдена");
            }
            await _attendanceService.DeleteAttendance(id);
        }

        public async Task<bool> AttendanceExistsAsync(long id)
        {
            return await _attendanceService.AttendanceExists(id);
        }
    }
}