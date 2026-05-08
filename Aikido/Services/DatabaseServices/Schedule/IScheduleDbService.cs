using Aikido.Entities;
using Aikido.Exceptions;

namespace Aikido.Services.DatabaseServices.Schedule
{
    public interface IScheduleDbService
    {
        Task<ScheduleEntity> GetScheduleById(long id);
        Task<List<ScheduleEntity>> GetActiveSchedulesByGroup(long groupId);
        Task<List<ScheduleEntity>> GetSchedulesByGroup(long groupId);
        Task<List<ScheduleEntity>> GetAllSchedules();
        Task<bool> ScheduleExists(long id);
        Task CloseSchedule(long id);
        Task CloseGroupSchedules(long groupId);
        Task RecoverSchedule(long id);
    }
}
