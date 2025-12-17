using Aikido.Dto.Seminars.Members;
using Aikido.Dto.Seminars.Members.CoachEditRequest;
using Aikido.Dto.Seminars.Members.Creation;
using Aikido.Entities.Seminar.SeminarMemberRequest;
using Aikido.Exceptions;
using Aikido.Services;
using Aikido.Services.DatabaseServices.Seminar;
using System.Text.Json;

namespace Aikido.Application.Services
{
    public class SeminarCoachEditRequestAppService
    {
        private readonly SeminarCoachEditRequestDbService _requestDbService;
        private readonly ISeminarDbService _seminarDbService;
        private readonly PaymentService _paymentDbService;

        public SeminarCoachEditRequestAppService(
            SeminarCoachEditRequestDbService requestDbService,
            ISeminarDbService seminarDbService,
            PaymentService paymentService)
        {
            _requestDbService = requestDbService;
            _seminarDbService = seminarDbService;
            _paymentDbService = paymentService;
        }

        public async Task<List<SeminarMemberCoachRequestDto>> GetClubCoachRequestList(long seminarId, long clubId, long coachId)
        {
            var requests = await _requestDbService.GetCoachRequestsByClub(seminarId, clubId, coachId);
            return requests
                .Select(r => new SeminarMemberCoachRequestDto(r))
                .ToList();
        }

        public async Task<List<SeminarMemberRequestDto>> GetCoachRequestData(long id)
        {
            return await _requestDbService.GetCoachRequestData(id);
        }

        public async Task CreateCoachRequest(long seminarId, SeminarMemberCoachRequestListCreationDto request)
        {
            await _requestDbService.CreateCoachRequest(seminarId, request);
        }

        public async Task UpdateRequestByCoach(long requestId, SeminarMemberCoachRequestListCreationDto request)
        {
            await _requestDbService.UpdateRequestByCoach(requestId, request);
        }

        public async Task DeleteCoachRequest(long requestId)
        {
            await _requestDbService.DeleteRequest(requestId);
        }

        public async Task ApplyRequest(long requestId)
        {
            var request = await _requestDbService.GetCoachRequest(requestId);
            await _requestDbService.ApplyRequest(requestId);

            var list = new SeminarMemberCoachRequestListCreationDto(request);

            await _seminarDbService.CreateSeminarCoachMembers(request.SeminarId, list);
            foreach (var member in list.Members)
            {
                await _paymentDbService.CreateOrUpdateMemberPayments(request.SeminarId, member);
            }
        }

        public async Task RejectRequest(long requestId, string comment)
        {
            await _requestDbService.RejectRequest(requestId, comment);
        }

    }
}