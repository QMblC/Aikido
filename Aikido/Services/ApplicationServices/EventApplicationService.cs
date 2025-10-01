using Aikido.Dto;
using Aikido.Services;
using Aikido.Exceptions;

namespace Aikido.Application.Services
{
    public class EventApplicationService
    {
        private readonly EventService _eventService;

        public EventApplicationService(EventService eventService)
        {
            _eventService = eventService;
        }

        public async Task<EventDto> GetEventByIdAsync(long id)
        {
            var eventEntity = await _eventService.GetEventById(id);
            return new EventDto(eventEntity);
        }

        public async Task<List<EventDto>> GetAllEventsAsync()
        {
            var events = await _eventService.GetAllEvents();
            return events.Select(e => new EventDto(e)).ToList();
        }

        public async Task<long> CreateEventAsync(EventDto eventData)
        {
            return await _eventService.CreateEvent(eventData);
        }

        public async Task UpdateEventAsync(long id, EventDto eventData)
        {
            if (!await _eventService.EventExists(id))
            {
                throw new EntityNotFoundException($"Событие с Id = {id} не найдено");
            }
            await _eventService.UpdateEvent(id, eventData);
        }

        public async Task DeleteEventAsync(long id)
        {
            if (!await _eventService.EventExists(id))
            {
                throw new EntityNotFoundException($"Событие с Id = {id} не найдено");
            }
            await _eventService.DeleteEvent(id);
        }

        public async Task<List<EventDto>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var events = await _eventService.GetEventsByDateRange(startDate, endDate);
            return events.Select(e => new EventDto(e)).ToList();
        }

        public async Task<bool> EventExistsAsync(long id)
        {
            return await _eventService.EventExists(id);
        }
    }
}