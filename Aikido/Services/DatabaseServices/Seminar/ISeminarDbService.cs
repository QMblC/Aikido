using Aikido.Dto.Seminars;
using Aikido.Dto.Seminars.Creation;
using Aikido.Dto.Seminars.Members;
using Aikido.Dto.Seminars.Members.CoachEditRequest;
using Aikido.Entities.Seminar;
using Aikido.Entities.Seminar.SeminarFilters;
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
        Task<List<SeminarEntity>> GetAllAsync(TimeFilter filter);

        Task<List<SeminarMemberEntity>> GetSeminarMembersAsync(long seminarId);
        Task<SeminarEntity> CreateAsync(SeminarCreationDto seminarData);

        Task CreateSeminarSchedule(SeminarEntity seminar, List<SeminarScheduleCreationDto> schedule);
        Task UpdateSeminarSchedule(SeminarEntity seminar, List<SeminarScheduleCreationDto> schedule);

        Task<SeminarRegulationEntity> GetSeminarRegulation(long seminarId);
        Task CreateSeminarRegulationAsync(long seminarId, byte[] fileInBytes);
        Task DeleteSeminarRegulationAsync(long seminarId);
        Task UpdateAsync(long id, SeminarCreationDto seminarData);
        Task UpdateAsync(SeminarEntity seminar);
        Task DeleteAsync(long id);

        Task UpdateEditorList(long seminarId, List<long> editorIds);

        Task ApplySeminarResult(long seminarId);
        Task CancelSeminarResult(long seminarId);

        Task<List<SeminarMemberManagerRequestEntity>> GetManagerMembersAsync(long seminarId, long managerId);
        Task<List<SeminarMemberManagerRequestEntity>> GetManagerMembersByClubAsync(long seminarId, long clubId);
        Task CreateManagerMembersByClubAsync(long seminarId, SeminarMemberManagerRequestListDto managerRequest);
        Task DeleteManagerMembersByClubAsync(long seminarId, long managerId, long clubId);
        Task ConfirmManagerMembersByClubAsync(long seminarId, long managerId, long clubId);
        Task CancelManagerMemberByClubAsync(long seminarId, long managerId, long clubId);
        Task<List<SeminarMemberManagerRequestEntity>> GetRequestedMembers(long seminarId, bool isApplied = true);
        Task CreateSeminarMembersFromRequest(long seminarId);
        Task CreateSeminarMembers(long seminarId, SeminarMemberListDto memberList);
        Task<List<SeminarMemberManagerRequestEntity>> GetCoachMembersByClub(long seminarId, long clubId, long coachId);
        Task CreateSeminarCoachMembers(long seminarId, SeminarMemberCoachRequestListCreationDto memberList);
        Task<List<SeminarEntity>> GetUserSeminarHistory(long userId);
        Task<List<SeminarMemberEntity>> GetUserCertificationHistory(long userId);
    }
}
