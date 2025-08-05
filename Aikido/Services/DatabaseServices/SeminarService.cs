using Aikido.Data;
using Aikido.Dto.Seminars;
using Aikido.Entities.Seminar;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Aikido.Services.DatabaseServices
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

        public async Task<List<SeminarEntity>> GetSeminarList()
        {
            return await context.Seminars.ToListAsync();
        }

        public async Task<long> CreateSeminar(SeminarDto seminarData)
        {
            var seminarEntity = new SeminarEntity(seminarData);

            await context.Seminars.AddAsync(seminarEntity);

            await SaveChangesAsync();

            return seminarEntity.Id;
        }

        public async Task DeleteSeminar(long id)
        {
            var seminarEntity = await GetSeminar(id);

            context.Seminars.Remove(seminarEntity);

            await SaveChangesAsync();
        }

        public async Task UpdateSeminar(long id, SeminarDto seminarData)
        {
            var seminarEntity = await GetSeminar(id);

            seminarEntity.UpdateFromJson(seminarData);

            await SaveChangesAsync();
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
            throw new NotImplementedException();
        }

        public async Task DeleteSeminarMember(long memberId)
        {
            var memberEntity = await GetSeminarMember(memberId);

            context.SeminarMembers.Remove(memberEntity);

            await SaveChangesAsync();
        }

        public async Task UpdateSeminarMember(long memberId, SeminarMemberDto memberDto)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
        
        public async Task<List<StatementEntity>> GetSeminarCoachStatements(long seminarId)
        {
            var statements = await context.Statements
                .Where(statement => statement.SeminarId == seminarId)
                .ToListAsync();

            return statements;
        }

        public bool Contains(long seminarId, long coachId)
        {
            return context.Statements
                .Where(statement => statement.SeminarId == seminarId
                && statement.CoachId == coachId)
                .FirstOrDefault() != null;
        }

        public async Task<StatementEntity> GetCoachStatement(long seminarId, long coachId)
        {
            var statement =  await context.Statements
                .Where(statement => statement.SeminarId == seminarId
                && statement.CoachId == coachId)
                .FirstOrDefaultAsync();

            if (statement == null)
                throw new NotImplementedException("Значение ведомости не найдено в базе данных");

            return statement;
        }

        public async Task CreateSeminarCoachStatement(
            long seminarId,
            long coachId,
            byte[] table,
            string name)
        {
            var statement = new StatementEntity(seminarId, coachId, table, name);

            context.Add(statement);

            await SaveChangesAsync();
        }

        public async Task DeleteSeminarCoachStatement(long seminarId, long coachId)
        {
            var statement = await context.Statements
                .Where(statement => statement.SeminarId == seminarId
                && statement.CoachId== coachId)
                .FirstOrDefaultAsync();

            context.Statements.Remove(statement);

            await SaveChangesAsync();
        }

        public async Task UpdateSeminarCoachStatement(
            long seminarId,
            long coachId, 
            byte[] table,
            string name)
        {
            var statement = await context.Statements
                .Where(statement => statement.SeminarId == seminarId
                && statement.CoachId == coachId)
                .FirstOrDefaultAsync();

            statement.UpdateStatement(table, name);

            await SaveChangesAsync();
        }

        public async Task CreateFinalStatement(long seminarId, byte[] table)
        {
            var seminar = await GetSeminar(seminarId);

            seminar.FinalStatementPath = table;

            await SaveChangesAsync();
        }

        public async Task DeleteFinalStatement(long seminarId)
        {
            var seminar = await GetSeminar(seminarId);

            seminar.FinalStatementPath = null;

            await SaveChangesAsync();
        }

        public async Task UpdateAppliement(long seminarId, bool value)
        {
            var seminar = await GetSeminar(seminarId);

            seminar.IsFinalStatementApplied = value;

            await SaveChangesAsync();
        }
    }
}
