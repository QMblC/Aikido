using Aikido.Dto;
using Aikido.Services.DatabaseServices;
using Aikido.Exceptions;
using Aikido.Services;

namespace Aikido.Application.Services
{
    public class ScheduleApplicationService
    {
        private readonly ScheduleService _scheduleService;

        public ScheduleApplicationService(ScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        public async Task<ScheduleDto> GetScheduleByIdAsync(long id)
        {
            var schedule = await _scheduleService.GetScheduleById(id);
            return new ScheduleDto(schedule);
        }

        public async Task<List<ScheduleDto>> GetSchedulesByGroupAsync(long groupId)
        {
            var schedules = await _scheduleService.GetSchedulesByGroup(groupId);
            return schedules.Select(s => new ScheduleDto(s)).ToList();
        }

        public async Task<List<ScheduleDto>> GetAllSchedulesAsync()
        {
            var schedules = await _scheduleService.GetAllSchedules();
            return schedules.Select(s => new ScheduleDto(s)).ToList();
        }

        public async Task<bool> ScheduleExistsAsync(long id)
        {
            return await _scheduleService.ScheduleExists(id);
        }
    }
}