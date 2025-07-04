using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services
{
    public class ClubService
    {
        private readonly AppDbContext context;

        public ClubService(AppDbContext context)
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
                throw new Exception($"Ошибка при обработке клуба: {ex.InnerException?.Message}", ex);
            }
        }

        public async Task<ClubEntity> GetClubById(long id)
        {
            var clubEntity = await context.Clubs.FindAsync(id);
            if (clubEntity == null)
                throw new KeyNotFoundException($"Клуб с Id = {id} не найден.");

            return clubEntity;
        }


        public async Task<List<ClubEntity>> GetClubsList()
        {
            return await context.Clubs.OrderBy(club => club.Name)
                .ToListAsync();
        }

        public async Task<long> CreateClub(ClubDto clubData)
        {
            var clubEntity = new ClubEntity();
            clubEntity.UpdateFromJson(clubData);

            context.Clubs.Add(clubEntity);

            await SaveDb();

            return clubEntity.Id;
        }

        public async Task DeleteClub(long id)
        {
            var clubEntity = await context.Clubs.FindAsync(id);
            if (clubEntity == null)
                throw new KeyNotFoundException($"Клуб с Id = {id} не найден.");

            context.Remove(clubEntity);

            await SaveDb();

        }

        public async Task UpdateClub(long id, ClubDto clubNewData)
        {
            var clubEntity = await context.Clubs.FindAsync(id);
            if (clubEntity == null)
                throw new KeyNotFoundException($"Клуб с Id = {id} не найден.");

            clubEntity.UpdateFromJson(clubNewData);

            await SaveDb();
        }
    }
}