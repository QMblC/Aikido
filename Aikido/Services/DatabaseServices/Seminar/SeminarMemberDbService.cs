using Aikido.Data;
using Aikido.Dto.Seminars;
using Aikido.Entities.Seminar.SeminarMember;
using Aikido.Services.DatabaseServices.Base;

namespace Aikido.Services.DatabaseServices.Seminar
{
    public class SeminarMemberDbService : DbService<SeminarMemberEntity, SeminarMemberDbService>
    {
        public SeminarMemberDbService(AppDbContext context, ILogger<SeminarMemberDbService> logger) : base(context, logger)
        {
        }

        public SeminarMemberEntity GetBySeminarAndUserId(long seminarId, long userId)
        {
            var member = context.SeminarMembers
                .Where(member => member.SeminarId == seminarId
                && member.UserId == userId)
                .SingleOrDefault();

            return member ??
                throw new KeyNotFoundException($"Участник семинара " +
                $"с seminarId = {seminarId}, userId = {userId}");
        }

        public async Task DeleteBySeminarAndUserId(long seminarId, long userId)
        {
            var member = GetBySeminarAndUserId(seminarId, userId);

            context.SeminarMembers.Remove(member);

            await SaveChangesAsync();
        }
    }
}
