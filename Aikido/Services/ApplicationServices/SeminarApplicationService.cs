using Aikido.AdditionalData.Enums;
using Aikido.Dto.Seminars;
using Aikido.Dto.Seminars.Creation;
using Aikido.Dto.Seminars.Members;
using Aikido.Dto.Seminars.Members.Creation;
using Aikido.Dto.Users;
using Aikido.Entities;
using Aikido.Entities.Seminar;
using Aikido.Entities.Seminar.SeminarFilters;
using Aikido.Entities.Seminar.SeminarMemberRequest;
using Aikido.Entities.Users;
using Aikido.Exceptions;
using Aikido.Services;
using Aikido.Services.DatabaseServices.Group;
using Aikido.Services.DatabaseServices.Seminar;
using Aikido.Services.DatabaseServices.User;
using Aikido.Services.NotificationService;
using Aikido.Services.UnitOfWork;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Reflection;

namespace Aikido.Application.Services
{
    public class SeminarApplicationService
    {
        private readonly ISeminarDbService _seminarDbService;
        private readonly IUserDbService _userDbService;
        private readonly IUserMembershipDbService _userMembershipDbService;
        private readonly IGroupDbService _groupDbService;
        private readonly PaymentDbService _paymentDbService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public SeminarApplicationService(
            ISeminarDbService seminarDbService,
            IUserDbService userDbService,
            IUserMembershipDbService userMembershipDbService,
            IGroupDbService groupDbService,
            PaymentDbService paymentDbService,
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _seminarDbService = seminarDbService;
            _userDbService = userDbService;
            _userMembershipDbService = userMembershipDbService;
            _groupDbService = groupDbService;
            _paymentDbService = paymentDbService;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<SeminarDto> GetSeminarByIdAsync(long id)
        {
            var seminar = await _seminarDbService.GetByIdOrThrowException(id);
            return new SeminarDto(seminar);
        }

        public async Task<List<SeminarShortDto>> GetAllSeminarsAsync(TimeFilter filter)
        {
            filter = filter ?? new();

            var seminars = await _seminarDbService.GetAllAsync(filter);
            return seminars.Select(s => new SeminarShortDto(s)).ToList();
        }

        public async Task<long> CreateSeminarAsync(SeminarCreationDto seminarData)
        {
            if (seminarData.CreatorId == null)
            {
                throw new InvalidOperationException("Не указан создатель");
            }


            SeminarEntity seminar = null;
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                seminar = await _seminarDbService.CreateAsync(seminarData);
                await _unitOfWork.SaveChangesAsync();
                await _seminarDbService.CreateSeminarSchedule(seminar, seminarData.Schedule);
                await _seminarDbService.UpdateEditorList(seminar.Id, seminarData.Editors);
            });

            await _notificationService.SeminarDataChanged(NotificationAction.Create, seminar.Id);

            return seminar.Id;
        }

        public async Task UpdateSeminarAsync(long id, SeminarCreationDto seminarData)
        {
            if (!await _seminarDbService.Exists(id))
                throw new EntityNotFoundException($"Семинар с Id = {id} не найден");

            var seminar = await _seminarDbService.GetByIdOrThrowException(id);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _seminarDbService.UpdateAsync(id, seminarData);
                await _unitOfWork.SaveChangesAsync();
                await _seminarDbService.UpdateSeminarSchedule(seminar, seminarData.Schedule);
                await _seminarDbService.UpdateEditorList(id, seminarData.Editors);
            });

            await _notificationService.SeminarDataChanged(NotificationAction.Update, id);
        }

        public async Task DeleteSeminarAsync(long id)
        {
            if (!await _seminarDbService.Exists(id))
                throw new EntityNotFoundException($"Семинар с Id = {id} не найден");

            await _seminarDbService.DeleteAsync(id);

            await _notificationService.SeminarDataChanged(NotificationAction.Delete, id);
        }

        public async Task<List<SeminarMemberDto>> GetSeminarMembersAsync(long seminarId)
        {
            var members = await _seminarDbService.GetSeminarMembersAsync(seminarId);
            var membersDto = new List<SeminarMemberDto>();

            foreach(var member in members)
            {
                var payments = await _paymentDbService.GetSeminarMemberPayments(seminarId, member.UserId);
                membersDto.Add(new(member, payments));
            }
            return membersDto;
        }

        public async Task<byte[]> GetSeminarRegulationAsync(long seminarId)
        {
            var regulation = await _seminarDbService.GetSeminarRegulation(seminarId);

            return regulation.File;
        }

        public async Task AddSeminarRegulationAsync(long seminarId, byte[] fileInBytes)
        {
            await _seminarDbService.CreateSeminarRegulationAsync(seminarId, fileInBytes);
        }

        public async Task DeleteSeminarRegulationAsync(long seminarId)
        {
            await _seminarDbService.DeleteSeminarRegulationAsync(seminarId);
        }

        public async Task ApplySeminarResult(long seminarId)
        {
            var seminar = await _seminarDbService.GetByIdOrThrowException(seminarId);

            await _seminarDbService.ApplySeminarResult(seminarId);

            var members = await _seminarDbService.GetSeminarMembersAsync(seminarId);

            foreach (var member in members)
            {
                if (member.Status == SeminarMemberStatus.Certified)
                {
                    await _userDbService.UpdateUserGrade(member.UserId, member.CertificationGrade.Value);
                }
            }
            foreach (var payment in seminar.Payments.Where(p => p.Type == PaymentType.BudoPassport))
            {
                await _userDbService.UpdateUserBudoPassport(payment.UserId, true);
            }

            await _notificationService.UserDataChanged(NotificationAction.Update);
        }

        public async Task CancelSeminarResult(long seminarId)
        {
            var seminar = await _seminarDbService.GetByIdOrThrowException(seminarId);

            await _seminarDbService.CancelSeminarResult(seminarId);

            var members = await _seminarDbService.GetSeminarMembersAsync(seminarId);

            foreach (var member in members)
            {
                if (member.Status == SeminarMemberStatus.Certified)
                {
                    await _userDbService.UpdateUserGrade(member.UserId, member.OldGrade);
                }
            }
            foreach (var payment in seminar.Payments.Where(p => p.Type == PaymentType.BudoPassport))
            {
                await _userDbService.UpdateUserBudoPassport(payment.UserId, true);
            }

            await _notificationService.UserDataChanged(NotificationAction.Update);
        }

        #region ManagerRequest

        public async Task<List<SeminarMemberRequestDto>> GetRequestedMembers(long seminarId, long managerId)
        {
            var members = await _seminarDbService.GetManagerMembersAsync(seminarId, managerId);
            var membersDto = new List<SeminarMemberRequestDto>();

            foreach (var member in members)
            {
                var memberPayments = await _paymentDbService.GetSeminarMemberPayments(seminarId, member.UserId);
                membersDto.Add(new SeminarMemberRequestDto(member, memberPayments));
            }

            return membersDto;
        }

        public async Task<List<SeminarMemberRequestDto>> GetClubRequestedMembers(long seminarId, long clubId)
        {
            var members = await _seminarDbService.GetManagerMembersByClubAsync(seminarId, clubId);
            var membersDto = new List<SeminarMemberRequestDto>();

            foreach (var member in members)
            {
                var memberPayments = await _paymentDbService.GetSeminarMemberPayments(seminarId, member.UserId);
                membersDto.Add(new SeminarMemberRequestDto(member, memberPayments));
            }

            return membersDto;
        }

        public async Task<List<UserShortDto>> FindClubMemberByName(long clubId, string name)
        {
            var users = await _userDbService.FindClubMemberByName(clubId, name);
            return users.Select(u => new UserShortDto(u))
                .ToList();
        }

        public async Task<SeminarMemberRequestDto> GetNewSeminarMemberManagerRequest(long seminarId, long userId)
        {
            await EnsureUserNotRequested(seminarId, userId);

            var seminar = await _seminarDbService.GetByIdOrThrowException(seminarId);
            var mainUserMembership = _userMembershipDbService.GetMainUserMembership(userId);
            var payments = await _paymentDbService.GetFakeSeminarMemberPayment(seminarId, userId);

            if (mainUserMembership == null)
            {
                throw new InvalidOperationException("Пользователь должен находится в группе и иметь 1 главную группу");
            }

            if (mainUserMembership.Group?.MainCoachId == null)
            {
                throw new InvalidOperationException("У главной группы пользователя нет главного тренера");
            }

            return new SeminarMemberRequestDto(seminar, mainUserMembership, payments);
        }

        public async Task CreateManagerMembersByClubAsync(long seminarId, SeminarMemberManagerRequestListDto managerRequest)
        {
            await EnsureSeminarStatementsUnlocked(seminarId);
            EnsureMembersUnique(managerRequest.Members);

            await _seminarDbService.CreateManagerMembersByClubAsync(seminarId, managerRequest);
            foreach (var member in managerRequest.Members)
            {
                await _paymentDbService.CreateOrUpdateMemberPayments(seminarId, member);
            }

            await _notificationService.SeminarManagerMembersDataChanged(NotificationAction.Update, seminarId, managerRequest.ManagerId, managerRequest.ClubId);
        }

        public async Task ConfirmManagerMembersByClubAsync(long seminarId,
            long managerId,
            long clubId)
        {
            await EnsureSeminarStatementsUnlocked(seminarId);
            var members = await _seminarDbService.GetManagerMembersByClubAsync(seminarId, clubId);

            if (members.Count == 0)
            {
                throw new InvalidOperationException("Заявлено 0 участников");
            }

            await _seminarDbService.ConfirmManagerMembersByClubAsync(seminarId, managerId, clubId);
            await _notificationService.SeminarManagerMembersDataChanged(NotificationAction.Update, seminarId, managerId, clubId);
        }

        public async Task CancelManagerMemberByClubAsync(long seminarId,
            long managerId,
            long clubId)
        {
            await EnsureSeminarStatementsUnlocked(seminarId);

            await _seminarDbService.CancelManagerMemberByClubAsync(seminarId, managerId, clubId);
            await _notificationService.SeminarManagerMembersDataChanged(NotificationAction.Update, seminarId, managerId, clubId);
        }
        #endregion

        public async Task<List<SeminarMemberRequestDto>> GetAllManagerRequests(long seminarId)
        {
            var members = await _seminarDbService.GetRequestedMembers(seminarId);
            var membersDto = new List<SeminarMemberRequestDto>();

            foreach (var member in members)
            {
                var payments = await _paymentDbService.GetSeminarMemberPayments(seminarId, member.UserId);
                membersDto.Add(new(member, payments));
            }

            return membersDto; 
        }

        public async Task<List<ManagerRequest>> GetManagerRequestList(long seminarId)
        {
            var managers = await _userDbService.GetActiveManagers();
            var managerRequestList = new List<ManagerRequest>();

            foreach(var manager in managers)
            {
                var requestedMembers = await _seminarDbService.GetRequestedMembers(seminarId);
                requestedMembers = requestedMembers.Where(m => m.ManagerId  == manager.Id).ToList();
                managerRequestList.Add(new(new UserShortDto(manager), requestedMembers.Count));
            }

            return managerRequestList;
        }

        public async Task SetRequestedMembers(long seminarId)
        {
            await _seminarDbService.CreateSeminarMembersFromRequest(seminarId);
            await _notificationService.SeminarMembersDataChanged(NotificationAction.Update, seminarId);
        }

        public async Task<SeminarMemberDto> GetNewSeminarMember(long seminarId, long userId)
        {
            var seminar = await _seminarDbService.GetByIdOrThrowException(seminarId);
            var mainUserMembership = _userMembershipDbService.GetMainUserMembership(userId);
            var payments = await _paymentDbService.GetFakeSeminarMemberPayment(seminarId, userId);

            return new SeminarMemberDto(seminar, mainUserMembership, payments);
        }

        public async Task SaveSeminarMembers(long seminarId, SeminarMemberListDto request)
        {
            EnsureMembersUnique(request.Members);

            await _seminarDbService.CreateSeminarMembers(seminarId, request);
            foreach (var member in request.Members)
            {
                await _paymentDbService.CreateOrUpdateMemberPayments(seminarId, member);
            }

            await _notificationService.SeminarMembersDataChanged(NotificationAction.Update, seminarId);
        }

        public async Task<List<SeminarMemberRequestDto>> GetCoachMembersByClub(long seminarId, long clubId, long coachId)
        {
            var members = await _seminarDbService.GetCoachMembersByClub(seminarId, clubId, coachId);
            var membersDto = new List<SeminarMemberRequestDto>();

            foreach (var member in members)
            {
                var memberPayments = await _paymentDbService.GetSeminarMemberPayments(seminarId, member.UserId);
                membersDto.Add(new SeminarMemberRequestDto(member, memberPayments));
            }

            return membersDto;
        }

        public async Task<List<UserShortDto>> FindCoachMemberInClubByName(long clubId, long coachId, string name)
        {
            var users = await _userDbService.FindCoachMemberInClubByName(clubId, coachId, name);
            return users.Select(u => new UserShortDto(u))
                .ToList();
        }

        public async Task<bool> IsClubSeminarMembersManagerRequestConfirmed(long seminarId, long clubId)
        {
            var members = await _seminarDbService.GetManagerMembersByClubAsync(seminarId, clubId);

            if (members.Count == 0)
            {
                return false;
            }    

            return members.All(m => m.IsConfirmed);
        }

        private async Task EnsureSeminarStatementsUnlocked(long seminarId)
        {
            var seminar = await _seminarDbService.GetByIdOrThrowException(seminarId);

            if (seminar.AreStatementsBlocked)
            {
                throw new InvalidOperationException("Изменение ведомостей семинара заблокировано");
            }
        }

        public async Task BlockSeminarStatements(long seminarId)
        {
            var seminar = await _seminarDbService.GetByIdOrThrowException(seminarId);
            seminar.AreStatementsBlocked = true;
            await _seminarDbService.UpdateAsync(seminar);

            await _notificationService.SeminarDataChanged(NotificationAction.Update, seminarId);
        }

        public async Task UnlockSeminarStatements(long seminarId)
        {
            var seminar = await _seminarDbService.GetByIdOrThrowException(seminarId);
            seminar.AreStatementsBlocked = false;
            await _seminarDbService.UpdateAsync(seminar);
            await _notificationService.SeminarDataChanged(NotificationAction.Update, seminarId);
        }

        private async Task EnsureUserNotRequested(long seminarId, long userId)
        {
            var requestedMembers = await _seminarDbService.GetRequestedMembers(seminarId, false);

            if (requestedMembers.Any(m => m.UserId == userId))
            {
                throw new InvalidOperationException("Пользователь уже заявлен для участия");
            }
        }

        private void EnsureMembersUnique(IEnumerable<ISeminarMemberCreation> members)
        {
            var memberCount = members.Count();

            var distinctMemberCount = members.Select(m => m.UserId).Distinct().Count();

            if (memberCount != distinctMemberCount)
            {
                throw new InvalidOperationException("Пользователи повторяются");
            }
        }
    }
}
