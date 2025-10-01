using Aikido.Dto;
using Aikido.Entities;
using Aikido.Entities.Filters;
using Aikido.Exceptions;
using Aikido.Services.DatabaseServices.Club;
using Aikido.Services.DatabaseServices.Group;
using Aikido.Services.DatabaseServices.User;

namespace Aikido.Application.Services
{
    public class UserApplicationService
    {
        private readonly IUserDbService _userDbService;
        private readonly IClubDbService _clubDbService;
        private readonly IGroupDbService _groupDbService;

        public UserApplicationService(
            IUserDbService userDbService,
            IClubDbService clubDbService,
            IGroupDbService groupDbService)
        {
            _userDbService = userDbService;
            _clubDbService = clubDbService;
            _groupDbService = groupDbService;
        }

        public async Task<UserDto> GetUserByIdAsync(long id)
        {
            var user = await _userDbService.GetByIdOrThrowException(id);
            var userClubs = await _userDbService.GetUserClubsAsync(id);
            var userGroups = await _userDbService.GetUserGroupsAsync(id);

            return new UserDto(user, userClubs, userGroups);
        }

        public async Task<List<UserShortDto>> GetUserShortListAsync()
        {
            return await _userDbService.GetUserIdAndNamesAsync();
        }

        public async Task<List<UserShortDto>> FindUsersAsync(UserFilter filter)
        {
            var result = await _userDbService.GetUserListAlphabetAscending(0, 100, filter);
            return result.Users
                .Select(user => new UserShortDto { Id = user.Id.Value, Name = user.Name })
                .ToList();
        }

        public async Task<object> GetUserShortListCutDataAsync(int startIndex, int finishIndex, UserFilter filter)
        {
            var pagedResult = await _userDbService.GetUserListAlphabetAscending(startIndex, finishIndex, filter);
            var users = pagedResult.Users;

            foreach (var user in users)
            {
                var userClubs = await _userDbService.GetUserClubsAsync(user.Id.Value);
                var userGroups = await _userDbService.GetUserGroupsAsync(user.Id.Value);

                user.ClubNames = userClubs.Where(uc => uc.IsActive)
                    .Select(uc => uc.Club?.Name).ToList();
                user.GroupNames = userGroups.Where(ug => ug.IsActive)
                    .Select(ug => ug.Group?.Name).ToList();
            }

            return new
            {
                TotalCount = pagedResult.TotalCount,
                Users = users
            };
        }

        public async Task<long> CreateUserAsync(UserDto userData)
        {
            var userId = await _userDbService.CreateUser(userData);

            // Добавление в клубы
            if (userData.ClubIds != null && userData.ClubIds.Any())
            {
                foreach (var clubId in userData.ClubIds)
                {
                    if (!await _clubDbService.Exists(clubId))
                    {
                        throw new EntityNotFoundException($"Клуба с Id = {clubId} не существует");
                    }
                    await _userDbService.AddUserToClubAsync(userId, clubId);
                }
            }

            // Добавление в группы
            if (userData.GroupIds != null && userData.GroupIds.Any())
            {
                foreach (var groupId in userData.GroupIds)
                {
                    if (!await _groupDbService.Exists(groupId))
                    {
                        throw new EntityNotFoundException($"Группы с Id = {groupId} не существует");
                    }
                    await _userDbService.AddUserToGroupAsync(userId, groupId);
                }
            }

            return userId;
        }

        public async Task UpdateUserAsync(long id, UserDto userData)
        {
            var user = await _userDbService.GetByIdOrThrowException(id);
            await _userDbService.UpdateUser(id, userData);

            // Обновление клубов - удаляем старые и добавляем новые
            await _userDbService.RemoveUserFromAllClubsAsync(id);
            if (userData.ClubIds != null && userData.ClubIds.Any())
            {
                foreach (var clubId in userData.ClubIds)
                {
                    if (!await _clubDbService.Exists(clubId))
                    {
                        throw new EntityNotFoundException($"Клуба с Id = {clubId} не существует");
                    }
                    await _userDbService.AddUserToClubAsync(id, clubId);
                }
            }

            // Обновление групп
            await _userDbService.RemoveUserFromAllGroupsAsync(id);
            if (userData.GroupIds != null && userData.GroupIds.Any())
            {
                foreach (var groupId in userData.GroupIds)
                {
                    if (!await _groupDbService.Exists(groupId))
                    {
                        throw new EntityNotFoundException($"Группы с Id = {groupId} не существует");
                    }
                    await _userDbService.AddUserToGroupAsync(id, groupId);
                }
            }
        }

        public async Task DeleteUserAsync(long id)
        {
            // Удаляем пользователя из всех клубов и групп
            await _userDbService.RemoveUserFromAllClubsAsync(id);
            await _userDbService.RemoveUserFromAllGroupsAsync(id);
            await _userDbService.Delete(id);
        }

        public async Task<List<long>> CreateUsersAsync(List<UserDto> users)
        {
            var createdIds = new List<long>();

            foreach (var userData in users)
            {
                var userId = await CreateUserAsync(userData);
                createdIds.Add(userId);
            }

            return createdIds;
        }

        public async Task UpdateUsersAsync(List<UserDto> users)
        {
            foreach (var userData in users)
            {
                if (userData.Id.HasValue)
                {
                    await UpdateUserAsync(userData.Id.Value, userData);
                }
            }
        }

        public async Task<SeminarMemberDto> GetUserSeminarDataAsync(long userId)
        {
            var user = await _userDbService.GetByIdOrThrowException(userId);
            var userClubs = await _userDbService.GetUserClubsAsync(userId);
            var userGroups = await _userDbService.GetUserGroupsAsync(userId);

            if (!userClubs.Any() || !userGroups.Any())
            {
                throw new InvalidOperationException("Недостаточно информации о клубе или группе");
            }

            var primaryClub = userClubs.First(uc => uc.IsActive).Club;
            var primaryGroup = userGroups.First(ug => ug.IsActive).Group;

            UserEntity? coach = null;
            if (primaryGroup?.CoachId != null)
            {
                coach = await _userDbService.GetByIdOrThrowException(primaryGroup.CoachId.Value);
            }

            return new SeminarMemberDto(user, primaryClub, coach);
        }

        public async Task AddUserToClubAsync(long userId, long clubId)
        {
            if (!await _userDbService.Exists(userId))
            {
                throw new EntityNotFoundException($"Пользователя с Id = {userId} не существует");
            }

            if (!await _clubDbService.Exists(clubId))
            {
                throw new EntityNotFoundException($"Клуба с Id = {clubId} не существует");
            }

            await _userDbService.AddUserToClubAsync(userId, clubId);
        }

        public async Task RemoveUserFromClubAsync(long userId, long clubId)
        {
            await _userDbService.RemoveUserFromClubAsync(userId, clubId);
        }

        public async Task AddUserToGroupAsync(long userId, long groupId)
        {
            if (!await _userDbService.Exists(userId))
            {
                throw new EntityNotFoundException($"Пользователя с Id = {userId} не существует");
            }

            if (!await _groupDbService.Exists(groupId))
            {
                throw new EntityNotFoundException($"Группы с Id = {groupId} не существует");
            }

            await _userDbService.AddUserToGroupAsync(userId, groupId);
        }

        public async Task RemoveUserFromGroupAsync(long userId, long groupId)
        {
            await _userDbService.RemoveUserFromGroupAsync(userId, groupId);
        }

        public async Task<List<ClubDto>> GetUserClubsAsync(long userId)
        {
            var userClubs = await _userDbService.GetUserClubsAsync(userId);
            return userClubs.Where(uc => uc.IsActive && uc.Club != null)
                           .Select(uc => new ClubDto(uc.Club!))
                           .ToList();
        }

        public async Task<List<GroupDto>> GetUserGroupsAsync(long userId)
        {
            var userGroups = await _userDbService.GetUserGroupsAsync(userId);
            return userGroups.Where(ug => ug.IsActive && ug.Group != null)
                            .Select(ug => new GroupDto(ug.Group!))
                            .ToList();
        }
    }
}