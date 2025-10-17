using Aikido.AdditionalData;
using Aikido.Dto;
using Aikido.Dto.Seminars;
using Aikido.Entities;
using Aikido.Entities.Filters;
using Aikido.Exceptions;
using Aikido.Services.DatabaseServices.Club;
using Aikido.Services.DatabaseServices.Group;
using Aikido.Services.DatabaseServices.Seminar;
using Aikido.Services.DatabaseServices.User;
using DocumentFormat.OpenXml.Spreadsheet;

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
            var userMembership = await _userDbService.GetUserMembershipsAsync(user.Id);

            return new UserDto(user, userMembership);
        }

        public async Task<List<UserShortDto>> GetUserShortListAsync()
        {
            return await _userDbService.GetUserIdAndNamesAsync();
        }

        public async Task<List<UserShortDto>> FindUsersAsync(UserFilter filter)
        {
            var result = await _userDbService.GetUserListAlphabetAscending(0, 100, filter);
            return result.Users
                .Select(user => new UserShortDto { 
                    Id = user.Id.Value,
                    LastName = user.LastName,
                    FirstName = user.FirstName,
                    MiddleName = user.LastName,
                })
                .ToList();
        }

        public async Task<UsersDataDto> GetUserShortListCutDataAsync(int startIndex, int finishIndex, UserFilter filter)
        {
            var pagedResult = await _userDbService.GetUserListAlphabetAscending(startIndex, finishIndex, filter);
            var users = pagedResult.Users;

            foreach (var user in users)
            {
                var userMemberships = await _userDbService.GetUserMembershipsAsync(user.Id.Value);
                user.UserMembershipDtos = userMemberships.Select(um => new UserMembershipDto(um)).ToList();
            }

            return new UsersDataDto
            {
                TotalCount = pagedResult.TotalCount,
                Users = users
            };
        }

        public async Task<long> CreateUserAsync(UserDto userData)//Здесь нужно подключить UnitOfWork
        {
            var userId = await _userDbService.CreateUser(userData);

            if (userData.UserMembershipIds != null && userData.UserMembershipIds.Any())
            {
                foreach (var userMembership in userData.UserMembershipDtos)
                {
                    var clubId = userMembership.ClubId.Value;
                    var groupId = userMembership.GroupId.Value;

                    if (!await _clubDbService.Exists(clubId))
                    {
                        throw new EntityNotFoundException($"Клуба с Id = {clubId} не существует");
                    }
                    if (!await _groupDbService.Exists(groupId))
                    {
                        throw new EntityNotFoundException($"Группы с Id = {groupId} не существует");
                    }

                    await _userDbService.AddUserMembershipAsync(userId,
                        clubId,
                        groupId,
                        EnumParser.ConvertStringToEnum<Role>(userMembership.RoleInGroup));
                }
            }

            return userId;
        }

        public async Task UpdateUserAsync(long userId, UserDto userData)//Здесь нужно подключить UnitOfWork
        {
            var user = await _userDbService.GetByIdOrThrowException(userId);
            await _userDbService.UpdateUser(userId, userData);

            await _userDbService.RemoveUserMemberships(userId);

            if (userData.UserMembershipDtos != null && userData.UserMembershipDtos.Any())
            {
                foreach (var userMembership in userData.UserMembershipDtos)
                {
                    var clubId = userMembership.ClubId.Value;
                    var groupId = userMembership.GroupId.Value;

                    if (!await _clubDbService.Exists(clubId))
                    {
                        throw new EntityNotFoundException($"Клуба с Id = {clubId} не существует");
                    }
                    if (!await _groupDbService.Exists(groupId))
                    {
                        throw new EntityNotFoundException($"Группы с Id = {groupId} не существует");
                    }

                    await _userDbService.AddUserMembershipAsync(userId,
                        clubId,
                        groupId,
                        EnumParser.ConvertStringToEnum<Role>(userMembership.RoleInGroup));
                }
            } 
        }

        public async Task DeleteUserAsync(long id)
        {
            await _userDbService.RemoveUserMemberships(id);
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

        public async Task AddUserMembershipAsync(long userId, long clubId, long groupId, Role roleInGroup)
        {
            if (!await _userDbService.Exists(userId))
            {
                throw new EntityNotFoundException($"Пользователя с Id = {userId} не существует");
            }

            if (!await _groupDbService.Exists(groupId))
            {
                throw new EntityNotFoundException($"Группы с Id = {groupId} не существует");
            }

            await _userDbService.AddUserMembershipAsync(userId, groupId, clubId, roleInGroup);
        }

        public async Task RemoveUserMembershipAsync(long userId, long groupId)
        {
            await _userDbService.RemoveUserMembershipAsync(userId, groupId);
        }

        public async Task<List<UserMembershipDto>> GetUserMembershipsAsync(long userId)
        {
            var userGroups = await _userDbService.GetUserMembershipsAsync(userId);
            return userGroups.Select(um => new UserMembershipDto(um)).ToList();
        }
    }
}