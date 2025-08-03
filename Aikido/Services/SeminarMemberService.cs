using Aikido.Data;
using Aikido.Dto.Seminars;
using Aikido.Entities.Seminar;

namespace Aikido.Services
{
    public class SeminarMemberService : DbService
    {
        public SeminarMemberService(AppDbContext context) : base(context)
        {
        }

        public async Task<SeminarMemberEntity> GetSeminarMember(long memberId)
        {
            var member = await context.SeminarMembers.FindAsync(memberId);

            return member ??
                throw new KeyNotFoundException($"Участник семинара с Id = {memberId} не найден");
        }

        public async Task<SeminarMemberEntity> GetSeminarMember(long seminarId, long userId)
        {
            var member = context.SeminarMembers
                .Where(member => member.SeminarId == seminarId
                && member.UserId == userId)
                .SingleOrDefault();

            return member ??
                throw new KeyNotFoundException($"Участник семинара " +
                $"с seminarId = {seminarId}, userId = {userId}");
        }

        public async Task<long> CreateSeminarMember(SeminarMemberDto memberDto)
        {
            var memberEntity = new SeminarMemberEntity(memberDto);

            await context.SeminarMembers.AddAsync(memberEntity);

            await SaveChangesAsync();

            return memberEntity.Id;
        }

        public async Task DeleteSeminarMember(long memberId)
        {
            var member = await GetSeminarMember(memberId);

            context.SeminarMembers.Remove(member);

            await SaveChangesAsync();
        }

        public async Task DeleteSeminarMember(long seminarId, long userId)
        {
            var member = await GetSeminarMember(seminarId, userId);

            context.SeminarMembers.Remove(member);

            await SaveChangesAsync();
        }
    }
}
