using Aikido.AdditionalData.Enums;
using Aikido.Dto.Seminars.Members;
using Aikido.Dto.Seminars.Members.CoachEditRequest;
using Aikido.Dto.Seminars.Members.Creation;
using Aikido.Entities.Seminar.SeminarFilters;
using Aikido.Entities.Seminar.SeminarMemberRequest;
using Aikido.Entities.Users;
using Aikido.Exceptions;
using Aikido.Services;
using Aikido.Services.DatabaseServices.Club;
using Aikido.Services.DatabaseServices.Seminar;
using Aikido.Services.NotificationService;
using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Aikido.Application.Services
{
    public class SeminarCoachEditRequestAppService
    {
        private readonly SeminarCoachEditRequestDbService _requestDbService;
        private readonly ISeminarDbService _seminarDbService;
        private readonly PaymentService _paymentDbService;
        private readonly IClubDbService _clubDbService;
        private readonly INotificationService _notificationService;

        public SeminarCoachEditRequestAppService(
            SeminarCoachEditRequestDbService requestDbService,
            ISeminarDbService seminarDbService,
            PaymentService paymentService,
            IClubDbService clubDbService,
            INotificationService notificationService
            )
        {
            _requestDbService = requestDbService;
            _seminarDbService = seminarDbService;
            _paymentDbService = paymentService;
            _clubDbService = clubDbService;
            _notificationService = notificationService;
        }

        public async Task<List<SeminarMemberCoachRequestDto>> GetClubCoachRequestList(
            long seminarId,
            long clubId,
            long coachId,
            RequestResultFilter filter)
        {
            var requests = await _requestDbService.GetCoachRequestsByClub(seminarId, clubId, coachId);

            var result = UseFilter(requests, filter);

            return result
                .Select(r => new SeminarMemberCoachRequestDto(r))
                .ToList();
        }

        public async Task<List<SeminarMemberRequestDto>> GetCoachRequestData(long id)
        {
            return await _requestDbService.GetCoachRequestData(id);
        }

        public async Task CreateCoachRequest(long seminarId, SeminarMemberCoachRequestListCreationDto request)
        {
            await EnsureSeminarStatementsUnlocked(seminarId);

            await _requestDbService.CreateCoachRequest(seminarId, request);
            await _notificationService.SeminarCoachMembersDataChanged(NotificationAction.Create,
                seminarId,
                request.CoachId.Value,
                request.ClubId.Value);
        }

        public async Task UpdateRequestByCoach(long requestId, SeminarMemberCoachRequestListCreationDto request)
        {
            var requestEntity = await _requestDbService.GetCoachRequest(requestId);
            await EnsureRequestPending(requestId);
            await EnsureSeminarStatementsUnlocked(requestEntity.SeminarId);

            await _requestDbService.UpdateRequestByCoach(requestId, request);
            await _notificationService.SeminarCoachMembersDataChanged(NotificationAction.Update,
                requestEntity.SeminarId,
                request.CoachId.Value,
                request.ClubId.Value);
        }

        public async Task DeleteCoachRequest(long requestId)
        {
            var requestEntity = await _requestDbService.GetCoachRequest(requestId);
            await EnsureSeminarStatementsUnlocked(requestEntity.SeminarId);

            await _requestDbService.DeleteRequest(requestId);
            await _notificationService.SeminarCoachMembersDataChanged(NotificationAction.Delete,
                requestEntity.SeminarId,
                requestEntity.RequestedById,
                requestEntity.ClubId);
        }

        public async Task ApplyRequest(long requestId, long reviewerId)
        {

            var request = await _requestDbService.GetCoachRequest(requestId);

            await EnsureRequestPending(requestId);
            await EnsureSeminarStatementsUnlocked(request.SeminarId);

            var list = new SeminarMemberCoachRequestListCreationDto(request);

            await _seminarDbService.CreateSeminarCoachMembers(request.SeminarId, list);
            foreach (var member in list.Members)
            {
                await _paymentDbService.CreateOrUpdateMemberPayments(request.SeminarId, member);
            }

            await _requestDbService.ApplyRequest(requestId, reviewerId);
            await _notificationService.SeminarCoachMembersDataChanged(NotificationAction.Update,
                request.SeminarId,
                request.RequestedById,
                request.ClubId);
        }

        public async Task RejectRequest(long requestId, long reviewerId, string comment)
        {       
            var request = await _requestDbService.GetCoachRequest(requestId);

            await EnsureRequestPending(requestId);
            await EnsureSeminarStatementsUnlocked(request.SeminarId);

            await _requestDbService.RejectRequest(requestId, reviewerId, comment);
            await _notificationService.SeminarCoachMembersDataChanged(NotificationAction.Update,
                request.SeminarId,
                request.RequestedById,
                request.ClubId);
        }

        public async Task<List<SeminarMemberCoachRequestDto>> GetCoachRequests(long seminarId, long clubId, RequestResultFilter filter)
        {
            filter = filter ?? new RequestResultFilter();
            var requests = await _requestDbService.GetCoachRequests(seminarId, clubId);

            var result = UseFilter(requests, filter); 

            return result.Select(r => new SeminarMemberCoachRequestDto(r)).ToList();
        }

        private List<SeminarMemberCoachRequestEntity> UseFilter(List<SeminarMemberCoachRequestEntity> requests, RequestResultFilter filter)
        {
            var result = new List<SeminarMemberCoachRequestEntity>();
            filter = filter ?? new RequestResultFilter();


            if (filter.IsPending)
            {
                result.AddRange(requests.Where(r => r.Status == Entities.Users.RequestStatus.Pending));
            }
            if (filter.IsReviewed)
            {
                result.AddRange(requests.Where(r => r.Status != Entities.Users.RequestStatus.Pending));
            }

            return result;
        }

        private async Task EnsureRequestPending(long requestId)
        {
            var request = await _requestDbService.GetCoachRequest(requestId);

            if (request.Status != RequestStatus.Pending)
            {
                throw new InvalidOperationException("Заявка уже рассмотрена");
            }
        }

        private async Task EnsureSeminarStatementsUnlocked(long seminarId)
        {
            var seminar = await _seminarDbService.GetByIdOrThrowException(seminarId);

            if (seminar.AreStatementsBlocked)
            {
                throw new InvalidOperationException("Изменение ведомостей семинара заблокировано");
            }
        }
    }
}