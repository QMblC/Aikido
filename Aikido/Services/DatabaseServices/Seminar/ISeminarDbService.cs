using Aikido.Dto.Seminars;
using Aikido.Dto.Seminars.Creation;
using Aikido.Dto.Seminars.Members;
using Aikido.Entities.Seminar;
using Aikido.Entities.Seminar.SeminarMember;
using Aikido.Entities.Seminar.SeminarMemberRequest;
using Aikido.Entities.Users;
using Aikido.Exceptions;

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

        Task UpdateEditorList(long seminarId, List<long> editorIds);

        Task AddSeminarMembersAsync(long seminarId, SeminarMemberListDto membersDto);
        Task SetFinalSeminarMembersAsync(long seminarId, List<FinalSeminarMemberDto> members);
        Task RemoveMemberAsync(long seminarId, long userId);
        Task<bool> IsMemberAsync(long seminarId, long userId);
        Task<int> GetMemberCountAsync(long seminarId);

        Task<List<SeminarEntity>> GetSeminarsByDateRangeAsync(DateTime startDate, DateTime endDate);

        Task<List<SeminarGroupEntity>> GetSeminarGroups(long seminarId);
        Task ApplySeminarResult(long seminarId);
        Task CancelSeminarResult(long seminarId);
        Task<SeminarMemberEntity> GetSeminarMemberAsync(long seminarId, long userId);
        Task<List<SeminarMemberEntity>> GetCoachMembersAsync(long seminarId, long coachId);

        Task<List<SeminarMemberManagerRequestEntity>> GetManagerMembersAsync(long seminarId, long managerId);
        Task<List<SeminarMemberManagerRequestEntity>> GetManagerMembersByClubAsync(long seminarId, long managerId, long clubId);
        Task CreateManagerMembersByClubAsync(long seminarId, SeminarMemberManagerRequestListDto managerRequest);
        Task DeleteManagerMembersByClubAsync(long seminarId, long managerId, long clubId);
        Task ConfirmManagerMembersByClubAsync(long seminarId, long managerId, long clubId);
        Task CancelManagerMemberByClubAsync(long seminarId, long managerId, long clubId);
        Task InitializeSeminar(long seminarId);
        Task<List<SeminarMemberManagerRequestEntity>> GetRequestedMembers(long seminarId);
        Task CreateSeminarMembersFromRequest(long seminarId);
        Task CreateSeminarMembers(long seminarId, SeminarMemberListDto memberList);
    }
}
