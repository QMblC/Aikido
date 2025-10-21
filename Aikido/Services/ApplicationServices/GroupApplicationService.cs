using Aikido.Dto;
using Aikido.Services.DatabaseServices.Group;
using Aikido.Services.DatabaseServices.User;
using Aikido.Services.DatabaseServices.Club;
using Aikido.Exceptions;
using Aikido.Services.UnitOfWork;
using Aikido.AdditionalData;

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
            var members = await _groupDbService.GetGroupMembersAsync(id);

            var memberDtos = members.Where(m => m.User != null)
                                  .Select(m => new UserShortDto(m.User!))
                                  .ToList();

            var coach = group.CoachId != null
                ? await _userDbService.GetByIdOrThrowException(group.CoachId.Value)
                : null;

            var club = group.ClubId != null
                ? await _clubDbService.GetByIdOrThrowException(group.ClubId.Value)
                : null;

            return new GroupInfoDto
            {
                Id = group.Id,
                Name = group.Name,
                AgeGroup = group.AgeGroup.ToString(),
                CoachId = group.CoachId,
                CoachName = coach?.FullName,
                ClubId = group.ClubId,
                ClubName = club?.Name,
                GroupMembers = memberDtos
            };
        }

        public async Task<List<GroupDto>> GetAllGroupsAsync()
        {
            var groups = await _groupDbService.GetAllAsync();
            return groups.Select(g => new GroupDto(g)).ToList();
        }

        public async Task<List<GroupDto>> GetGroupsByUserAsync(long userId)
        {
            var userGroups = await _userDbService.GetUserMembershipsAsync(userId);
            return userGroups.Where(ug => ug.Group != null)
                           .Select(ug => new GroupDto(ug.Group!))
                           .ToList();
        }

        public async Task<long> CreateGroupAsync(GroupDto groupData)
        {
            if (groupData.CoachId != null && !await _userDbService.Exists(groupData.CoachId.Value))
            {
                throw new EntityNotFoundException($"Тренера с Id = {groupData.CoachId} не существует");
            }

            if (groupData.ClubId != null && !await _clubDbService.Exists(groupData.ClubId.Value))
            {
                throw new EntityNotFoundException($"Клуба с Id = {groupData.ClubId} не существует");
            }

            
            return await _groupDbService.CreateAsync(groupData);
        }


        public async Task UpdateGroupAsync(long id, GroupDto groupData)
        {
            if (!await _groupDbService.Exists(id))
            {
                throw new EntityNotFoundException($"Группа с Id = {id} не найдена");
            }

            if (groupData.CoachId != null && !await _userDbService.Exists(groupData.CoachId.Value))
            {
                throw new EntityNotFoundException($"Тренера с Id = {groupData.CoachId} не существует");
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

        public async Task AddUserToGroupAsync(long groupId, long userId, Role roleInGroup)
        {
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

            var clubId = group.ClubId.Value;

            if (!await _clubDbService.Exists(clubId))
            {
                throw new EntityNotFoundException($"Клуба с Id = {clubId} не существует");
            }


            await _userDbService.AddUserMembershipAsync(userId, groupId, clubId, roleInGroup);
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