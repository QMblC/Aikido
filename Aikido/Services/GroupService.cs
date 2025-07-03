using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services
{
    public class GroupService
    {
        private readonly AppDbContext context;

        public GroupService(AppDbContext context)
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
                throw new Exception($"Ошибка при обработке группы: {ex.InnerException?.Message}", ex);
            }
        }

        public async Task<GroupEntity> GetGroupById(long id)
        {
            var groupEntity = await context.Groups.FindAsync(id);
            if (groupEntity == null)
                throw new KeyNotFoundException($"Клуб с Id = {id} не найден.");

            return groupEntity;
        }

        public async Task<long> CreateGroup(GroupDto groupData)
        {
            var groupEntity = new GroupEntity();
            groupEntity.UpdateFromJson(groupData);

            context.Groups.Add(groupEntity);

            await SaveDb();

            return groupEntity.Id;
        }

        public async Task DeleteGroup(long id)
        {
            var groupEntity = await context.Groups.FindAsync(id);
            if (groupEntity == null)
                throw new KeyNotFoundException($"Клуб с Id = {id} не найден.");

            context.Remove(groupEntity);

            await SaveDb();
        }
    }
}
