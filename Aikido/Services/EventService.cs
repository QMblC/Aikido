using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using Aikido.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services
{
    public class EventService
    {
        private readonly AppDbContext _context;

        public EventService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<EventEntity> GetEventById(long id)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Group)
                .Include(e => e.Club)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventEntity == null)
            {
                throw new EntityNotFoundException($"Событие с Id = {id} не найдено");
            }
            return eventEntity;
        }

        public async Task<List<EventEntity>> GetAllEvents()
        {
            return await _context.Events
                .Include(e => e.Group)
                .Include(e => e.Club)
                .Where(e => e.IsActive)
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();
        }

        public async Task<List<EventEntity>> GetEventsByDateRange(DateTime startDate, DateTime endDate)
        {
            return await _context.Events
                .Include(e => e.Group)
                .Include(e => e.Club)
                .Where(e => e.StartDate >= startDate && e.StartDate <= endDate && e.IsActive)
                .OrderBy(e => e.StartDate)
                .ToListAsync();
        }

        public async Task<List<EventEntity>> GetEventsByGroup(long groupId)
        {
            return await _context.Events
                .Include(e => e.Group)
                .Include(e => e.Club)
                .Where(e => e.GroupId == groupId && e.IsActive)
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();
        }

        public async Task<List<EventEntity>> GetEventsByClub(long clubId)
        {
            return await _context.Events
                .Include(e => e.Group)
                .Include(e => e.Club)
                .Where(e => e.ClubId == clubId && e.IsActive)
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();
        }

        public async Task<long> CreateEvent(EventDto eventData)
        {
            var eventEntity = new EventEntity(eventData);
            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();
            return eventEntity.Id;
        }

        public async Task UpdateEvent(long id, EventDto eventData)
        {
            var eventEntity = await GetEventById(id);
            eventEntity.UpdateFromJson(eventData);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEvent(long id)
        {
            var eventEntity = await GetEventById(id);
            eventEntity.IsActive = false;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> EventExists(long id)
        {
            return await _context.Events.AnyAsync(e => e.Id == id);
        }
    }
}
