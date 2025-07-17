using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services
{
    public class SeminarService : DbService
    {
        public SeminarService(AppDbContext context) : base(context)
        {
        }

        public async Task<SeminarEntity> GetSeminar(long id)
        {
            var seminarEntity = context.Seminars.FindAsync(id).Result;
            if (seminarEntity == null)
            {
                throw new KeyNotFoundException($"Семинар с Id = {id} не найден");
            }

            return seminarEntity;
        }

        public async Task<long> CreateSeminar(SeminarDto seminarData)
        {
            var seminarEntity = new SeminarEntity(seminarData);

            await context.Seminars.AddAsync(seminarEntity);

            await SaveDb();

            return seminarEntity.Id;
        }

        public async Task DeleteSeminar(long id)
        {
            var seminarEntity = await GetSeminar(id);

            context.Seminars.Remove(seminarEntity);

            await SaveDb();
        }

        public async Task UpdateSeminar(long id, SeminarDto seminarData)
        {
            var seminarEntity = await GetSeminar(id);

            seminarEntity.UpdateFromJson(seminarData);

            await SaveDb();
        }

        public async Task<SeminarMemberEntity> GetSeminarMember(long memberId)
        {
            var memberEntity = await context.SeminarMembers.FindAsync(memberId);
            if (memberEntity == null)
                throw new KeyNotFoundException($"Участник семинара с Id = {memberId} не найден");

            return memberEntity;
        }

        public async Task<long> CreateSeminarMember(SeminarMemberDto seminarMemberDto)
        {
            var memberEntity = new SeminarMemberEntity(seminarMemberDto);

            await context.SeminarMembers.AddAsync(memberEntity);

            await SaveDb();

            return memberEntity.Id;
        }

        public async Task DeleteSeminarMember(long memberId)
        {
            var memberEntity = await GetSeminarMember(memberId);

            context.SeminarMembers.Remove(memberEntity);

            await SaveDb();
        }

        public async Task UpdateSeminarMember(long memberId, SeminarMemberDto memberDto)
        {
            var memberEntity = await GetSeminarMember(memberId);

            memberEntity.UpdateFromJson(memberDto);

            await SaveDb();
        }

        public async Task<List<SeminarMemberEntity>> GetMembersBySeminarId(long seminarId)
        {
            var seminarDate = GetSeminar(seminarId).Result.Date;

            return await context.SeminarMembers
                .Where(member => member.CertificationDate.Equals(seminarDate))
                .ToListAsync();
        }

        public async Task CreateSeminarMember(List<SeminarMemberDto> members)
        {
            foreach (var member in members)
            {
                await CreateSeminarMember(member);
            }
        }
    }
}
