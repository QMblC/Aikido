using Aikido.Dto.Seminars;
using Aikido.Dto.Seminars.Creation;
using Aikido.Entities.Seminar;

namespace Aikido.Services.DatabaseServices.Seminar
{
    public interface ISeminarDbService
    {
        Task<SeminarEntity> GetByIdOrThrowException(long id);
        Task<bool> Exists(long id);
        Task<List<SeminarEntity>> GetAllAsync();

        Task<List<SeminarMemberEntity>> GetSeminarMembersAsync(long seminarId);

        Task<long> CreateAsync(SeminarDto seminarData);
        Task<SeminarRegulationEntity> GetSeminarRegulation(long seminarId);
        Task CreateSeminarRegulationAsync(long seminarId, byte[] fileInBytes);
        Task DeleteSeminarRegulationAsync(long seminarId);
        Task UpdateAsync(long id, SeminarDto seminarData);
        Task DeleteAsync(long id);

        Task AddSeminarMembersAsync(long seminarId, List<SeminarMemberCreationDto> membersDto);
        Task RemoveMemberAsync(long seminarId, long userId);
        Task<bool> IsMemberAsync(long seminarId, long userId);
        Task<int> GetMemberCountAsync(long seminarId);

        Task<List<SeminarEntity>> GetSeminarsByDateRangeAsync(DateTime startDate, DateTime endDate);

        Task<List<SeminarGroupEntity>> GetSeminarGroups(long seminarId);
    }
}
