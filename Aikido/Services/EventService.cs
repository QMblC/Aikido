using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services
{
    public class EventService
    {
        private readonly AppDbContext context;

        public EventService(AppDbContext context)
        {
            this.context = context;
        }

        private async Task SaveDb()
        {
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при обработке мероприятия: {ex.InnerException?.Message}", ex);
            }
        }

        public async Task<ClubEntity> GetEventById(long id)
        {
            var eventEntity = await context.Clubs.FindAsync(id);
            if (eventEntity == null)
                throw new KeyNotFoundException($"Мероприятие с Id = {id} не найден.");

            return eventEntity;
        }

        public async Task<long> CreateEvent(EventDto eventData)
        {
            var eventEntity = new EventEntity();
            eventEntity.UpdateFromJson(eventData);

            context.Events.Add(eventEntity);

            await SaveDb();

            return eventEntity.Id;
        }

        public async Task DeleteEvent(long id)
        {
            var eventEntity = await context.Events.FindAsync(id);
            if (eventEntity == null)
                throw new KeyNotFoundException($"Мероприятие с Id = {id} не найден.");

            context.Remove(eventEntity);

            await SaveDb();

        }

        public async Task UpdateEvent(long id, EventDto eventNewData)
        {
            var eventEntity = await context.Events.FindAsync(id);
            if (eventEntity == null)
                throw new KeyNotFoundException($"Мероприятие с Id = {id} не найден.");

            eventEntity.UpdateFromJson(eventNewData);

            await SaveDb();
        }
    }
}