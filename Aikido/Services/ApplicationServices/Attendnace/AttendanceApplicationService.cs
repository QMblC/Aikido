using Aikido.Services.DatabaseServices;
using Aikido.Exceptions;
using Aikido.Dto.Attendance;
using Aikido.Services.DatabaseServices.User;
using Aikido.Services.NotificationService;
using Aikido.AdditionalData.Enums;
using Aikido.Services.DatabaseServices.Attendance;
using Aikido.Entities.Users;

namespace Aikido.Services.ApplicationServices.Attendnace
{
    public class AttendanceApplicationService : IAttendanceApplicationService
    {
        private readonly IAttendanceDbService _attendanceDbService;
        private readonly IUserDbService _userDbService;
        private readonly IUserMembershipDbService _userMembershipDbService;
        private readonly INotificationService _notificationService;

        public AttendanceApplicationService(IAttendanceDbService attendanceService,
            IUserDbService userDbService,
            IUserMembershipDbService userMembershipDbService,
            INotificationService notificationService)
        {
            _attendanceDbService = attendanceService;
            _userDbService = userDbService;
            _userMembershipDbService = userMembershipDbService;
            _notificationService = notificationService;
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

        public async Task<long> MarkAttendanceAsync(long groupId, AttendanceCreationDto attendanceData)
        {
            //if (attendanceData.Date > DateTime.UtcNow)
            //{
            //    throw new InvalidOperationException("Невозможно поставить отметку за в будущую дату");
            //}

            var userMembership = _userMembershipDbService.GetActiveUserMembership(attendanceData.UserId, groupId);
            return await _attendanceDbService.CreateAttendance(userMembership.Id, attendanceData.Date);
        }

        public async Task UpdateAttendances(
            long groupId,
            AttendanceUpdateDto attendanceData)
        {
            var groupedAttendances = new Dictionary<long, List<DateTime>>();

            foreach (var attendance in attendanceData.ToCreate)
            {
                UserMembershipEntity? userMembership = null;

                try
                {
                    userMembership =
                    _userMembershipDbService.GetUserMembership(
                        attendance.UserId,
                        groupId,
                        attendanceData.ToCreate.FirstOrDefault().Date);
                }
                catch (Exception ex)
                {
                    continue;
                }

                if (!groupedAttendances.ContainsKey(userMembership.Id))
                {
                    groupedAttendances[userMembership.Id] = new List<DateTime>();
                }

                groupedAttendances[userMembership.Id]
                    .Add(attendance.Date);
            }

            await _attendanceDbService.DeleteAttendances(attendanceData.ToDelete);
            await _attendanceDbService.CreateAttendances(groupedAttendances);
            await _notificationService.AttendanceDataChanged(NotificationAction.Update, groupId);
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