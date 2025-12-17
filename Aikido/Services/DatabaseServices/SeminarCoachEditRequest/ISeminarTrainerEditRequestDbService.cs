using Aikido.Dto.Seminars.Members.TrainerEditRequest;
using Aikido.Entities.Seminar.SeminarMemberRequest;

namespace Aikido.Services.DatabaseServices.Seminar
{
    public interface ISeminarTrainerEditRequestDbService
    {
        Task CreateInitialRequestsAsync(long seminarId);
        Task<List<SeminarMemberTrainerEditRequestEntity>> GetTrainerRequestsByClubAsync(
            long seminarId, 
            long trainerId, 
            long clubId);
        Task<List<SeminarMemberTrainerEditRequestEntity>> GetTrainerAllRequestsAsync(
            long seminarId, 
            long trainerId);
        Task<List<SeminarMemberTrainerEditRequestEntity>> GetManagerRequestsByClubAsync(
            long seminarId, 
            long managerId, 
            long clubId);
        Task<List<SeminarMemberTrainerEditRequestEntity>> GetPendingRequestsAsync(long seminarId);
        Task UpdateRequestStatusAsync(long requestId, 
            string status, 
            string? comment = null);
        Task SaveTrainerRequestsAsync(long seminarId, 
            long trainerId, 
            long clubId,
            List<SeminarMemberTrainerEditRequestCreationDto> newRequests);
        Task ApplyRequestAsync(long requestId);
        Task<SeminarMemberTrainerEditRequestEntity> GetByIdOrThrowException(long id);
    }
}