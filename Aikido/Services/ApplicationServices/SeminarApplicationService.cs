using Aikido.Dto.Seminars;
using Aikido.Dto.Seminars.Creation;
using Aikido.Entities;
using Aikido.Entities.Seminar;
using Aikido.Exceptions;
using Aikido.Services;
using Aikido.Services.DatabaseServices.Group;
using Aikido.Services.DatabaseServices.Seminar;
using Aikido.Services.DatabaseServices.User;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;

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
            return await _seminarDbService.CreateAsync(seminarData);
        }



        public async Task UpdateSeminarAsync(long id, SeminarDto seminarData)
        {
            if (!await _seminarDbService.Exists(id))
                throw new EntityNotFoundException($"Семинар с Id = {id} не найден");

            await _seminarDbService.UpdateAsync(id, seminarData);
        }

        public async Task DeleteSeminarAsync(long id)
        {
            if (!await _seminarDbService.Exists(id))
                throw new EntityNotFoundException($"Семинар с Id = {id} не найден");

            await _seminarDbService.DeleteAsync(id);
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
                var memberData = memberGroup.Members.First(m => m.UserId == member.UserId);

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

        public async Task<List<SeminarMemberStartDataDto>> GetStartMembersData(long seminarId, long coachId)//ToDo проверять оплату AnnualFee
        {
            var seminar = await _seminarDbService.GetByIdOrThrowException(seminarId);

            var coachMemberships = await _userDbService.GetUserMembershipsAsync(coachId);

            var coachMembers = new List<UserEntity>();

            foreach (var cm in coachMemberships)
            {
                var groupMembers = await _groupDbService.GetGroupMembersAsync(cm.GroupId);
                coachMembers.AddRange(groupMembers.Select(um => um.User));
            }

            return coachMembers
                .Select(u => new SeminarMemberStartDataDto(u, seminar))
                .ToList();
        }

        public async Task<SeminarMemberStartDataDto> GetStartMemberdata(long seminarId, long userId)//ToDo проверять оплату AnnualFee
        {
            var seminar = await _seminarDbService.GetByIdOrThrowException(seminarId);
            var user = await _userDbService.GetByIdOrThrowException(userId);

            return new SeminarMemberStartDataDto(user, seminar);
        }
    }
}
