using Aikido.Services.DatabaseServices.Group;
using Aikido.Services.DatabaseServices.User;
using Aikido.Services.DatabaseServices.Club;
using Aikido.Exceptions;
using Aikido.Services.UnitOfWork;
using Aikido.AdditionalData;
using Aikido.Dto.Users;
using Aikido.Dto.Groups;
using Aikido.Dto.Users.Creation;

namespace Aikido.Application.Services
{
    public class GroupApplicationService
    {
        private readonly IGroupDbService _groupDbService;
        private readonly IUserDbService _userDbService;
        private readonly IClubDbService _clubDbService;
        private readonly IUnitOfWork _unitOfWork;

        public GroupApplicationService(
            IGroupDbService groupDbService,
            IUserDbService userDbService,
            IClubDbService clubDbService,
            IUnitOfWork unitOfWork)
        {
            _groupDbService = groupDbService;
            _userDbService = userDbService;
            _clubDbService = clubDbService;
            _unitOfWork = unitOfWork;
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
            var groups = await _groupDbService.GetAllAsync();
            return groups.Select(g => new GroupDto(g)).ToList();
        }

        public async Task<List<GroupDto>> GetGroupsByUserAsync(long userId)
        {
            var userMemberships = await _userDbService.GetUserMembershipsAsync(userId);
            return userMemberships.Where(ug => ug.Group != null)
                           .Select(um => new GroupDto(um.Group!))
                           .ToList();
        }
        
        public async Task<List<GroupShortDto>> GetGroupsByCoach(long coachId)
        {
            var coachGroups = await _userDbService.GetUserMembershipsAsync(coachId);
            return coachGroups.Where(ug => ug.Group != null
                && ug.RoleInGroup == Role.Coach)
                .Select(ug => new GroupShortDto(ug.Group))
                .ToList();
        }

        public async Task<long> CreateGroupAsync(GroupCreationDto groupData)
        {
            if (groupData.ClubId != null && !await _clubDbService.Exists(groupData.ClubId.Value))
            {
                throw new EntityNotFoundException($"Клуба с Id = {groupData.ClubId} не существует");
            }

            return await _groupDbService.CreateAsync(groupData);
        }

        public async Task UpdateGroupAsync(long id, GroupCreationDto groupData)
        {
            if (!await _groupDbService.Exists(id))
            {
                throw new EntityNotFoundException($"Группа с Id = {id} не найдена");
            }


            if (groupData.ClubId != null && !await _clubDbService.Exists(groupData.ClubId.Value))
            {
                throw new EntityNotFoundException($"Клуба с Id = {groupData.ClubId} не существует");
            }

            await _groupDbService.UpdateAsync(id, groupData);
        }

        public async Task DeleteGroupAsync(long id)
        {
            if (!await _groupDbService.Exists(id))
            {
                throw new EntityNotFoundException($"Группа с Id = {id} не найдена");
            }

            await _groupDbService.RemoveAllMembersFromGroupAsync(id);
            await _groupDbService.DeleteAsync(id);
        }

        public async Task AddUserToGroupAsync(long userId, UserMembershipCreationDto userMembership)
        {
            var groupId = userMembership.GroupId.Value;
            var clubId = userMembership.ClubId.Value;

            if (!await _groupDbService.Exists(groupId))
            {
                throw new EntityNotFoundException($"Группы с Id = {groupId} не существует");
            }

            if (!await _userDbService.Exists(userId))
            {
                throw new EntityNotFoundException($"Пользователя с Id = {userId} не существует");
            }

            var group = await _groupDbService.GetGroupById(groupId);

            if (group.ClubId == null)
            {
                throw new Exception($"У группы нет клуба");
            }

            if (!await _clubDbService.Exists(clubId))
            {
                throw new EntityNotFoundException($"Клуба с Id = {clubId} не существует");
            }


            await _userDbService.AddUserMembershipAsync(userId, userMembership);
        }

        public async Task RemoveUserFromGroupAsync(long groupId, long userId)
        {
            await _userDbService.RemoveUserMembershipAsync(userId, groupId);
        }

        public async Task<bool> GroupExistsAsync(long id)
        {
            return await _groupDbService.Exists(id);
        }

        public async Task<List<UserShortDto>> GetGroupMembersAsync(long groupId)
        {
            var members = await _groupDbService.GetGroupMembersAsync(groupId);
            return members.Where(m => m.User != null)
                         .Select(m => new UserShortDto(m.User!))
                         .ToList();
        }
    }
}