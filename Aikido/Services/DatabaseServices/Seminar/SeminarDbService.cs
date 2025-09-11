using Aikido.Data;
using Aikido.Dto.Seminars;
using Aikido.Entities.Seminar;
using Aikido.Services.DatabaseServices.Base;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Aikido.Services.DatabaseServices.Seminar
{
    public class SeminarDbService : DbService<SeminarEntity, SeminarDbService>, ISeminarDbService
    {
        public SeminarDbService(AppDbContext context, ILogger<SeminarDbService> logger) : base(context, logger)
        {

        }

        public async Task<List<SeminarMemberEntity>> GetMembersBySeminarId(long seminarId)
        {
            var seminar = await GetByIdOrThrowException(seminarId);

            return await context.SeminarMembers
                .Where(member => member.Certification.Date.Equals(seminar.Date))
                .ToListAsync();
        }
        
        public async Task<List<SeminarCoachStatementEntity>> GetSeminarCoachStatements(long seminarId)
        {
            var statements = await context.Statements
                .Where(statement => statement.SeminarId == seminarId)
                .ToListAsync();

            return statements;
        }

        public bool Exists(long seminarId, long coachId)
        {
            return context.Statements
                .Where(statement => statement.SeminarId == seminarId
                && statement.CoachId == coachId)
                .SingleOrDefault() != null;
        }

        public async Task<SeminarCoachStatementEntity> GetCoachStatement(long seminarId, long coachId)
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
            var statement = new SeminarCoachStatementEntity(seminarId, coachId, table, name);

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
            var seminar = await GetByIdOrThrowException(seminarId);

            seminar.FinalStatementPath = table;

            await SaveChangesAsync();
        }

        public async Task DeleteFinalStatement(long seminarId)
        {
            var seminar = await GetByIdOrThrowException(seminarId);

            seminar.FinalStatementPath = null;

            await SaveChangesAsync();
        }

        public async Task UpdateAppliement(long seminarId, bool value)
        {
            var seminar = await GetByIdOrThrowException(seminarId);

            seminar.IsFinalStatementApplied = value;

            await SaveChangesAsync();
        }
    }
}
