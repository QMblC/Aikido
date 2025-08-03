using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services.DatabaseServices
{
    public class ClubService : DbService
    {
        public ClubService(AppDbContext context) : base(context) { }

        public async Task<ClubEntity> GetClubById(long id)
        {
            var clubEntity = await context.Clubs.FindAsync(id);
            if (clubEntity == null)
                throw new KeyNotFoundException($"Клуб с Id = {id} не найден.");

            return clubEntity;
        }

        public async Task<bool> Contains(long id)
        {
            var clubEntity = await context.Clubs.FindAsync(id);
            return clubEntity != null;
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

            await SaveChangesAsync();

            return clubEntity.Id;
        }

        public async Task DeleteClub(long id)
        {
            var clubEntity = await context.Clubs.FindAsync(id);
            if (clubEntity == null)
                throw new KeyNotFoundException($"Клуб с Id = {id} не найден.");

            context.Remove(clubEntity);

            await SaveChangesAsync();

        }

        public async Task UpdateClub(long id, ClubDto clubNewData)
        {
            var clubEntity = await context.Clubs.FindAsync(id);
            if (clubEntity == null)
                throw new KeyNotFoundException($"Клуб с Id = {id} не найден.");

            clubEntity.UpdateFromJson(clubNewData);

            await SaveChangesAsync();
        }

        public async Task AddGroupToClub(long clubId, long groupId)
        {
            var clubEntity = await context.Clubs.FindAsync(clubId);
            if (clubEntity == null)
                throw new KeyNotFoundException($"Клуб с Id = {clubId} не найден.");

            var groupList = clubEntity.Groups.ToList();

            if (!groupList.Contains(groupId))
                groupList.Add(groupId); 

            clubEntity.Groups = groupList.ToArray();

            await SaveChangesAsync();
        }

        public async Task DeleteGroupFromClub(long clubId, long groupId)
        {
            var clubEntity = await context.Clubs.FindAsync(clubId);
            if (clubEntity == null)
                throw new KeyNotFoundException($"Клуб с Id = {clubId} не найден.");

            var groupList = clubEntity.Groups.ToList();

            if (!groupList.Remove(groupId))
                throw new KeyNotFoundException($"Группа с Id = {groupId} не найдена в клубе.");

            clubEntity.Groups = groupList.ToArray();

            await SaveChangesAsync();
        }
    }
}