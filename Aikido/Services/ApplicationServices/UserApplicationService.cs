using Aikido.AdditionalData;
using Aikido.AdditionalData.Enums;
using Aikido.Dto.Seminars;
using Aikido.Dto.Users;
using Aikido.Dto.Users.Creation;
using Aikido.Entities;
using Aikido.Entities.Filters;
using Aikido.Entities.Users;
using Aikido.Exceptions;
using Aikido.Services.ApplicationServices;
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
        private readonly UserMembershipApplicationService _userMembershipApplicationService;

        public UserApplicationService(
            IUserDbService userDbService,
            IClubDbService clubDbService,
            IGroupDbService groupDbService,
            UserMembershipApplicationService userMembershipApplicationService)
        {
            _userDbService = userDbService;
            _clubDbService = clubDbService;
            _groupDbService = groupDbService;
            _userMembershipApplicationService = userMembershipApplicationService;
        }

        public async Task<UserDto> GetUserByIdAsync(long id)
        {
            var user = await _userDbService.GetByIdOrThrowException(id);
            var userMembership = await _userDbService.GetActiveUserMembershipsAsync(user.Id);

            return new UserDto(user, userMembership);
        }

        public async Task<List<UserShortDto>> GetUserShortListAsync()
        {
            return await _userDbService.GetUserIdAndNamesAsync();
        }

        public async Task<List<UserShortDto>> GetCoachStudentsByName(long coachId, string name)
        {
            var users = await _userDbService.GetCoachStudentByName(coachId, name);
            return users.Select(u => new UserShortDto(u))
                .ToList();
        }

        public async Task<List<UserShortDto>> FindUsersAsync(UserFilter filter)
        {
            var result = await _userDbService.GetUserListAlphabetAscending(0, 100, filter);
            return result.Users
                .Select(user => new UserShortDto(user))
                .ToList();
        }

        public async Task<UsersDataDto> GetUserShortListCutDataAsync(int startIndex, int finishIndex, UserFilter filter)
        {
            var pagedResult = await _userDbService.GetUserListAlphabetAscending(startIndex, finishIndex, filter);
            var users = pagedResult.Users;

            foreach (var user in users)
            {
                var userMemberships = await _userDbService.GetActiveUserMembershipsAsync(user.Id.Value);
                user.UserMembershipDtos = userMemberships.Select(um => new UserMembershipDto(um)).ToList();
            }

            return new UsersDataDto
            {
                TotalCount = pagedResult.TotalCount,
                Users = users
            };
        }

        public async Task<long> CreateUserAsync(UserCreationDto userData)//Здесь нужно подключить UnitOfWork
        {
            var userId = await _userDbService.CreateUser(userData);

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
                    if (!await _groupDbService.ExistsActive(groupId))
                    {
                        throw new EntityNotFoundException($"Группы с Id = {groupId} не существует");
                    }

                    await _userMembershipApplicationService.AddUserMembershipAsync(userId, userMembership);
                }
            }

            return userId;
        }

        public async Task UpdateUserAsync(long userId, UserCreationDto userData)//Здесь нужно подключить UnitOfWork
        {//Переделать
            var user = await _userDbService.GetByIdOrThrowException(userId);
            await _userDbService.UpdateUser(userId, userData);

            await _userDbService.RemoveUserMemberships(userId);//Не удалять

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
                    if (!await _groupDbService.ExistsActive(groupId))
                    {
                        throw new EntityNotFoundException($"Группы с Id = {groupId} не существует");
                    }

                    await _userMembershipApplicationService.AddUserMembershipAsync(userId, userMembership);
                }
            } 
        }

        public async Task DeleteUserAsync(long id)
        {
            await _userDbService.RemoveUserMemberships(id);
            await _userDbService.Delete(id);
        }

        public async Task<List<long>> CreateUsersAsync(List<UserCreationDto> users)
        {
            var createdIds = new List<long>();

            foreach (var userData in users)
            {
                var userId = await CreateUserAsync(userData);
                createdIds.Add(userId);
            }

            return createdIds;
        }

        private async Task EnsureUserExists(long userId)
        {
            if (!await _userDbService.Exists(userId))
            {
                throw new EntityNotFoundException($"Пользователя с Id = {userId} не существует");
            }
        }

        private async Task EnsureGroupExists(long groupId)
        {
            if (!await _groupDbService.ExistsActive(groupId))
            {
                throw new EntityNotFoundException($"Группы с Id = {groupId} не существует");
            }
        }
    }
}