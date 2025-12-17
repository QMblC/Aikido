using Aikido.Dto.Seminars.Members.TrainerEditRequest;
using Aikido.Entities.Seminar.SeminarMemberRequest;
using Aikido.Exceptions;
using Aikido.Services.DatabaseServices.Seminar;

namespace Aikido.Application.Services
{
    public class SeminarTrainerEditRequestAppService
    {
        private readonly ISeminarTrainerEditRequestDbService _requestDbService;
        private readonly ISeminarDbService _seminarDbService;

        public SeminarTrainerEditRequestAppService(
            ISeminarTrainerEditRequestDbService requestDbService,
            ISeminarDbService seminarDbService)
        {
            _requestDbService = requestDbService;
            _seminarDbService = seminarDbService;
        }


        public async Task<List<SeminarMemberTrainerEditRequestDto>> GetTrainerRequestsByClubAsync(
            long seminarId, long trainerId, long clubId)
        {
            var requests = await _requestDbService.GetTrainerRequestsByClubAsync(
                seminarId, trainerId, clubId);

            return requests.Select(r => new SeminarMemberTrainerEditRequestDto(r)).ToList();
        }

        public async Task<List<SeminarMemberTrainerEditRequestDto>> GetTrainerAllRequestsAsync(
            long seminarId, long trainerId)
        {
            var requests = await _requestDbService.GetTrainerAllRequestsAsync(seminarId, trainerId);
            return requests.Select(r => new SeminarMemberTrainerEditRequestDto(r)).ToList();
        }

        public async Task<List<SeminarMemberTrainerEditRequestDto>> GetManagerRequestsByClubAsync(
            long seminarId, long managerId, long clubId)
        {
            var requests = await _requestDbService.GetManagerRequestsByClubAsync(
                seminarId, managerId, clubId);

            return requests.Select(r => new SeminarMemberTrainerEditRequestDto(r)).ToList();
        }

        public async Task<List<SeminarMemberTrainerEditRequestDto>> GetPendingRequestsAsync(
            long seminarId)
        {
            var requests = await _requestDbService.GetPendingRequestsAsync(seminarId);
            return requests.Select(r => new SeminarMemberTrainerEditRequestDto(r)).ToList();
        }

        public async Task<List<TrainerRequestGroupDto>> GetGroupedPendingRequestsAsync(long seminarId)
        {
            var requests = await _requestDbService.GetPendingRequestsAsync(seminarId);

            var grouped = requests
                .GroupBy(r => new { r.TrainerId, r.ClubId })
                .Select(g => new TrainerRequestGroupDto
                {
                    TrainerId = g.Key.TrainerId,
                    TrainerName = g.First().Trainer?.FirstName + " " + g.First().Trainer?.LastName,
                    ClubId = g.Key.ClubId,
                    ClubName = g.First().Club?.Name,
                    PendingRequestsCount = g.Count(),
                    Requests = g.Select(r => new SeminarMemberTrainerEditRequestDto(r)).ToList()
                })
                .ToList();

            return grouped;
        }

        public async Task SendTrainerRequestsAsync(long seminarId, long trainerId, long clubId,
            List<SeminarMemberTrainerEditRequestCreationDto> membersList)
        {
            // Валидация
            if (membersList == null)
                throw new ArgumentNullException(nameof(membersList));

            // membersList может быть пуста, если тренер удалил всех участников

            await _requestDbService.SaveTrainerRequestsAsync(
                seminarId, trainerId, clubId, membersList);
        }

        public async Task SendTrainerRequestsByClubAsync(
            SeminarMemberTrainerEditRequestListDto requestList)
        {
            if (requestList == null)
                throw new ArgumentNullException(nameof(requestList));

            await _requestDbService.SaveTrainerRequestsAsync(
                requestList.SeminarId,
                requestList.TrainerId,
                requestList.ClubId,
                requestList.Members);
        }

        public async Task ReviewRequestAsync(long requestId,
            SeminarMemberTrainerEditRequestReviewDto reviewDto)
        {
            if (reviewDto == null)
                throw new ArgumentNullException(nameof(reviewDto));

            var status = reviewDto.IsApproved ? "approved" : "rejected";
            var comment = reviewDto.IsApproved ? null : reviewDto.Comment;

            await _requestDbService.UpdateRequestStatusAsync(requestId, status, comment);

            // Если одобрено, применяем заявку
            if (reviewDto.IsApproved)
            {
                await _requestDbService.ApplyRequestAsync(requestId);
            }
        }

        public async Task ApproveTrainerClubRequestsAsync(long seminarId, long trainerId, long clubId)
        {
            var requests = await _requestDbService.GetManagerRequestsByClubAsync(
                seminarId, trainerId, clubId);

            foreach (var request in requests)
            {
                var reviewDto = new SeminarMemberTrainerEditRequestReviewDto
                {
                    RequestId = request.Id,
                    IsApproved = true
                };

                await ReviewRequestAsync(request.Id, reviewDto);
            }
        }

        public async Task<SeminarMemberTrainerEditRequestDto> GetRequestDetailsAsync(long requestId)
        {
            var request = await _requestDbService.GetByIdOrThrowException(requestId);
            return new SeminarMemberTrainerEditRequestDto(request);
        }

        public async Task<TrainerRequestStatsDto> GetTrainerStatsAsync(long seminarId, long trainerId)
        {
            var requests = await _requestDbService.GetTrainerAllRequestsAsync(seminarId, trainerId);

            var stats = new TrainerRequestStatsDto
            {
                TrainerId = trainerId,
                TotalRequests = requests.Count,
                PendingRequests = requests.Count(r => r.Status == TrainerEditRequestStatus.Pending),
                ApprovedRequests = requests.Count(r => r.Status == TrainerEditRequestStatus.Approved),
                RejectedRequests = requests.Count(r => r.Status == TrainerEditRequestStatus.Rejected),
                AppliedRequests = requests.Count(r => r.Status == TrainerEditRequestStatus.Applied),
                AddRequests = requests.Count(r => r.RequestType == TrainerEditRequestType.Add),
                UpdateRequests = requests.Count(r => r.RequestType == TrainerEditRequestType.Update),
                DeleteRequests = requests.Count(r => r.RequestType == TrainerEditRequestType.Delete)
            };

            return stats;
        }
    }
}