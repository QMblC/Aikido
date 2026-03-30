using Aikido.AdditionalData.Enums;
using Aikido.Dto.Groups;
using Aikido.Dto.Users;
using Aikido.Dto.Users.Creation;
using Aikido.Exceptions;
using Aikido.Services;
using Aikido.Services.ApplicationServices;
using Aikido.Services.DatabaseServices.Club;
using Aikido.Services.DatabaseServices.Group;
using Aikido.Services.DatabaseServices.User;
using Aikido.Services.UnitOfWork;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace Aikido.Application.Services
{
    public class GroupApplicationService
    {
        private readonly IGroupDbService _groupDbService;
        private readonly IUserDbService _userDbService;
        private readonly IUserMembershipDbService _userMembershipDbService;
        private readonly IClubDbService _clubDbService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ScheduleDbService _scheduleDbService;
        private readonly UserMembershipApplicationService _userMembershipApplicationService;

        public GroupApplicationService(
            IGroupDbService groupDbService,
            IUserDbService userDbService,
            IUserMembershipDbService userMembershipDbService,
            IClubDbService clubDbService,
            IUnitOfWork unitOfWork,
            ScheduleDbService scheduleDbService,
            UserMembershipApplicationService userMembershipApplicationService)
        {
            _groupDbService = groupDbService;
            _userDbService = userDbService;
            _userMembershipDbService = userMembershipDbService;
            _clubDbService = clubDbService;
            _unitOfWork = unitOfWork;
            _scheduleDbService = scheduleDbService;
            _userMembershipApplicationService = userMembershipApplicationService;
        }

        public async Task<GroupDto> GetGroupByIdAsync(long id)
        {
            var group = await _groupDbService.GetByIdOrThrowException(id);
            return new GroupDto(group);
        }

        public async Task<GroupInfoDto> GetGroupInfoAsync(long id)
        {
            var group = await _groupDbService.GetByIdOrThrowException(id);

            return new GroupInfoDto(group);
        }

        public async Task<List<GroupDto>> GetAllGroupsAsync()
        {
            var groups = await _groupDbService.GetAllActiveAsync();
            return groups.Select(g => new GroupDto(g)).ToList();
        }

        public async Task<List<GroupDto>> GetGroupsByUserAsync(long userId)
        {
            var userMemberships = await _userMembershipDbService.GetActiveUserMembershipsAsync(userId);
            return userMemberships.Where(ug => ug.Group != null)
                .Select(um => new GroupDto(um.Group!))
                .ToList();
        }
        
        public async Task<List<GroupShortDto>> GetGroupsByCoach(long coachId)
        {
            var coachGroups = await _userMembershipDbService.GetActiveUserMembershipsAsync(coachId);
            return coachGroups.Where(ug => ug.Group != null
                && ug.RoleInGroup == Role.Coach)
                .Select(ug => new GroupShortDto(ug.Group))
                .ToList();
        }

        public async Task<long> CreateGroupAsync(GroupCreationDto groupData)
        {
            await EnsureClubExists(groupData.ClubId.Value);
            EnsureMainCoachIsDedicated(groupData);

            return await _groupDbService.CreateAsync(groupData);
        }

        public async Task UpdateGroupAsync(long id, GroupCreationDto groupData)
        {
            await EnsureGroupExists(id);
            await EnsureClubExists(groupData.ClubId.Value);
            EnsureMainCoachIsDedicated(groupData);

            await _groupDbService.UpdateAsync(id, groupData);
        }

        public async Task CloseGroupAsync(long id)
        {
            await _scheduleDbService.CloseGroupSchedules(id);

            await _groupDbService.CloseAsync(id);
        }

        public async Task RecoverGroupAsync(long id)
        {
            await _groupDbService.RecoverAsync(id);
        }

        public async Task DeleteGroupAsync(long id)
        {
            await EnsureGroupExists(id);

            await _groupDbService.RemoveAllMembersFromGroupAsync(id);
            await _groupDbService.DeleteAsync(id);
        }

        public async Task AddUserToGroupAsync(long userId, UserMembershipCreationShortDto dto)
        {
            var groupId = dto.GroupId;

            await EnsureGroupExists(groupId);
            await EnsureUserExists(userId);

            var group = await _groupDbService.GetGroupByIdAsync(groupId);

            if (group.ClubId == null)
            {
                throw new Exception($"У группы нет клуба");
            }
            var userMembership = new UserMembershipCreationDto(group, dto.IsMain, dto.IsCoach);

            await _userMembershipApplicationService.AddUserMembershipAsync(userId, userMembership);
        }

        public async Task RemoveUserFromGroupAsync(long groupId, long userId)
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () => 
            { 
                await _userMembershipApplicationService.CloseUserMembershipAsync(userId, groupId); 
            });
        }

        public async Task<bool> GroupExistsAsync(long id)
        {
            return await _groupDbService.ExistsActive(id);
        }

        public async Task<List<UserShortDto>> GetGroupMembersAsync(long groupId)
        {
            var members = await _groupDbService.GetGroupMembersAsync(groupId);
            return members.Where(m => m.User != null)
                         .Select(m => new UserShortDto(m.User!))
                         .ToList();
        }

        private async Task EnsureUserExists(long userId)
        {
            if (!await _userDbService.ExistsActive(userId))
            {
                throw new EntityNotFoundException($"Пользователя с Id = {userId} не существует");
            }
        }

        private void EnsureMainCoachIsDedicated(GroupCreationDto groupData)
        {
            if (groupData.MainCoachId == null && groupData.Coaches.Count == 0)
            {
                throw new InvalidOperationException("Не указан главный тренер");
            }
        }

        private async Task EnsureGroupExists(long groupId)
        {
            if (!await _groupDbService.ExistsActive(groupId))
            {
                throw new EntityNotFoundException($"Группы с Id = {groupId} не существует");
            }
        }

        private async Task EnsureClubExists(long clubId)
        {
            if (!await _clubDbService.ExistsActive(clubId))
            {
                throw new EntityNotFoundException($"Группы с Id = {clubId} не существует");
            }
        }
    }
}