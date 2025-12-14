using Aikido.Dto.Seminars;
using Aikido.Dto.Seminars.Creation;
using Aikido.Dto.Seminars.Members;
using Aikido.Dto.Users;
using Aikido.Entities;
using Aikido.Entities.Seminar;
using Aikido.Entities.Users;
using Aikido.Exceptions;
using Aikido.Services;
using Aikido.Services.DatabaseServices.Group;
using Aikido.Services.DatabaseServices.Seminar;
using Aikido.Services.DatabaseServices.User;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System.Reflection;

namespace Aikido.Application.Services
{
    public class SeminarApplicationService
    {
        private readonly ISeminarDbService _seminarDbService;
        private readonly IUserDbService _userDbService;
        private readonly IGroupDbService _groupDbService;
        private readonly PaymentService _paymentDbService;

        public SeminarApplicationService(
            ISeminarDbService seminarDbService,
            IUserDbService userDbService,
            IGroupDbService groupDbService,
            PaymentService paymentDbService)
        {
            _seminarDbService = seminarDbService;
            _userDbService = userDbService;
            _groupDbService = groupDbService;
            _paymentDbService = paymentDbService;
        }

        public async Task<SeminarDto> GetSeminarByIdAsync(long id)
        {
            var seminar = await _seminarDbService.GetByIdOrThrowException(id);
            return new SeminarDto(seminar);
        }

        public async Task<List<SeminarDto>> GetAllSeminarsAsync()
        {
            var seminars = await _seminarDbService.GetAllAsync();
            return seminars.Select(s => new SeminarDto(s)).ToList();
        }

        public async Task<long> CreateSeminarAsync(SeminarDto seminarData)
        {

            var seminarId = await _seminarDbService.CreateAsync(seminarData);
            await _seminarDbService.UpdateEditorList(seminarId, seminarData.Editors);
            await _seminarDbService.InitializeSeminar(seminarId);

            return seminarId;
        }

        public async Task UpdateSeminarAsync(long id, SeminarDto seminarData)
        {
            if (!await _seminarDbService.Exists(id))
                throw new EntityNotFoundException($"Семинар с Id = {id} не найден");

            await _seminarDbService.UpdateAsync(id, seminarData);
            await _seminarDbService.UpdateEditorList(id, seminarData.Editors);
        }

        public async Task DeleteSeminarAsync(long id)
        {
            if (!await _seminarDbService.Exists(id))
                throw new EntityNotFoundException($"Семинар с Id = {id} не найден");

            await _seminarDbService.DeleteAsync(id);
        }

        public async Task UpdateSeminarEditors(long seminarId, List<long> editorIds)
        {
            await _seminarDbService.UpdateEditorList(seminarId, editorIds);
        }

        public async Task<List<SeminarMemberDto>> GetSeminarMembersAsync(long seminarId)
        {
            var members = await _seminarDbService.GetSeminarMembersAsync(seminarId);
            return members.Select(sm => new SeminarMemberDto(sm)).ToList();
        }

        public async Task AddSeminarMembersAsync(long seminarId, SeminarMemberGroupDto memberGroup)
        {
            await _seminarDbService.AddSeminarMembersAsync(seminarId, memberGroup);

            var members = await _seminarDbService.GetSeminarMembersAsync(seminarId);
            

            foreach (var member in members)
            {
                var memberData = memberGroup.Members.FirstOrDefault(m => m.UserId == member.UserId);
                if (memberData == null) 
                {  
                    continue; 
                }
                await _paymentDbService.CreateSeminarMemberPayments(member, memberData);
            }
        }

        public async Task SetFinalSeminarMember(long seminarId, List<FinalSeminarMemberDto> membersDto)
        {
            await _seminarDbService.SetFinalSeminarMembersAsync(seminarId, membersDto);

            var members = await _seminarDbService.GetSeminarMembersAsync(seminarId);

            foreach (var member in members)
            {
                var memberData = membersDto.First(m => m.UserId == member.UserId);
                if (memberData == null)
                {
                    continue;
                }
                await _paymentDbService.CreateSeminarMemberPayments(member, memberData);
            }
        }

        public async Task RemoveMemberFromSeminarAsync(long seminarId, long userId)
        {
            await _seminarDbService.RemoveMemberAsync(seminarId, userId);
        }

        public async Task<bool> SeminarExistsAsync(long id)
        {
            return await _seminarDbService.Exists(id);
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

        public async Task<List<SeminarGroupDto>> GetSeminarGroups(long seminarId)
        {
            var groups = await _seminarDbService.GetSeminarGroups(seminarId);

            return groups.Select(g => new SeminarGroupDto(g))
                .ToList();
        }

        public async Task<SeminarMemberDto> GetStartMemberdata(long seminarId, long userId, long coachId)//ToDo проверять оплату AnnualFee
        {
            var seminar = await _seminarDbService.GetByIdOrThrowException(seminarId);
            var userMemberships = await _userDbService.GetUserMembershipsAsync(userId);
            var userMembership = userMemberships.Where(um => um.RoleInGroup == AdditionalData.Role.User
                && um.Group.UserMemberships
                .Any(um => um.UserId == coachId && um.RoleInGroup == AdditionalData.Role.Coach))
                .FirstOrDefault();
           
            if (userMembership == null)
            {
                throw new EntityNotFoundException(nameof(UserMembershipEntity));
            }

            if (await _seminarDbService.IsMemberAsync(seminarId, userId))
            {
                var member = await _seminarDbService.GetSeminarMemberAsync(seminarId, userId);
                return new SeminarMemberDto(member);
            }
            return new SeminarMemberDto(userMembership, seminar, await _paymentDbService.IsUserPayedAnnaulFee(userId, seminar.Date.Year));
        }

        public async Task ApplySeminarResult(long seminarId)
        {
            var seminar = await _seminarDbService.GetByIdOrThrowException(seminarId);

            await _seminarDbService.ApplySeminarResult(seminarId);

            var members = await _seminarDbService.GetSeminarMembersAsync(seminarId);

            foreach (var member in members)
            {
                if (member.Status == AdditionalData.SeminarMemberStatus.Certified)
                {
                    await _userDbService.UpdateUserGrade(member.UserId, member.CertificationGrade.Value);
                }
                if (member.User.HasBudoPassport == false && member.BudoPassportPayment?.Status == AdditionalData.PaymentStatus.Completed)
                {
                    await _userDbService.UpdateUserBudoPassport(member.UserId, true);
                }
            }
        }

        public async Task CancelSeminarResult(long seminarId)
        {
            var seminar = await _seminarDbService.GetByIdOrThrowException(seminarId);

            await _seminarDbService.CancelSeminarResult(seminarId);

            var members = await _seminarDbService.GetSeminarMembersAsync(seminarId);

            foreach (var member in members)
            {
                if (member.Status == AdditionalData.SeminarMemberStatus.Certified)
                {
                    await _userDbService.UpdateUserGrade(member.UserId, member.OldGrade);
                }
                if (member.User.HasBudoPassport == true && member.BudoPassportPayment?.Status == AdditionalData.PaymentStatus.Completed)
                {
                    await _userDbService.UpdateUserBudoPassport(member.UserId, false);
                }
            }
        }

        public async Task<List<UserShortDto>> GetRegisteredCoaches(long seminarId)
        {
            var seminarMembers = await _seminarDbService.GetSeminarMembersAsync(seminarId);
            var seminar = await _seminarDbService.GetByIdOrThrowException(seminarId);

            return seminarMembers
                .Select(sm => sm.Coach)
                .Where(sm => sm.CreatorId != seminar.CreatorId)
                .Distinct()
                .Select(c => new UserShortDto(c))
                .ToList();
        }

        public async Task<List<SeminarMemberDto>> GetStartMemberInfoByGroups(long seminarId, List<long> groupIds)
        {
            var seminar = await _seminarDbService.GetByIdOrThrowException(seminarId);

            var allMembersWithGroup = new List<(long GroupId, UserMembershipEntity Member)>();

            foreach (var groupId in groupIds)
            {
                var groupMembers = await _groupDbService.GetGroupMembersAsync(groupId);
                allMembersWithGroup.AddRange(groupMembers.Select(m => (GroupId: groupId, Member: m)));
            }

            var distinctMembers = allMembersWithGroup
                .GroupBy(x => x.Member.UserId)
                .Select(g => g.OrderBy(x => x.GroupId).First())
                .OrderBy(x => x.GroupId)
                .ToList();

            var result = new List<SeminarMemberDto>();

            foreach (var entry in distinctMembers)
            {

                if (await _seminarDbService.IsMemberAsync(seminarId, entry.Member.UserId))
                {
                    var existingMember = await _seminarDbService
                    .GetSeminarMemberAsync(seminarId, entry.Member.UserId);
                    result.Add(new SeminarMemberDto(existingMember));
                }
                else
                {
                    var isPaid = await _paymentDbService.IsUserPayedAnnaulFee(entry.Member.UserId, seminar.Date.Year);
                    result.Add(new SeminarMemberDto(entry.Member, seminar, isPaid));
                }
            }

            return result;
        }

        public async Task<List<SeminarMemberDto>> GetRegisteredCoachMembers(long seminarId, long coachId)
        {
            var coachMembersTasks = await _seminarDbService.GetCoachMembersAsync(seminarId, coachId);
            var coachMembers = coachMembersTasks
                .Select(sm => new SeminarMemberDto(sm))
                .ToList();

            return coachMembers;
        }

        public async Task<SeminarMemberDto> GetSeminarMemberAsync(long seminarId, long userId)
        {
            var memberEntity = await _seminarDbService.GetSeminarMemberAsync(seminarId, userId);
            return new SeminarMemberDto(memberEntity);
        }
    }
}
