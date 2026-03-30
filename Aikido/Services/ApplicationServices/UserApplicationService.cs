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
using Aikido.Services.UnitOfWork;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Aikido.Application.Services
{
    public class UserApplicationService
    {
        private readonly IUserDbService _userDbService;
        private readonly IUserMembershipDbService _userMembershipDbService;
        private readonly IClubDbService _clubDbService;
        private readonly IGroupDbService _groupDbService;
        private readonly UserMembershipApplicationService _userMembershipApplicationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISeminarDbService _seminarDbService;

        public UserApplicationService(
            IUserDbService userDbService,
            IUserMembershipDbService userMembershipDbService,
            IClubDbService clubDbService,
            IGroupDbService groupDbService,
            UserMembershipApplicationService userMembershipApplicationService,
            IUnitOfWork unitOfWork,
            ISeminarDbService seminarDbService)
        {
            _userDbService = userDbService;
            _userMembershipDbService = userMembershipDbService;
            _clubDbService = clubDbService;
            _groupDbService = groupDbService;
            _userMembershipApplicationService = userMembershipApplicationService;
            _unitOfWork = unitOfWork;
            _seminarDbService = seminarDbService;
        }

        public async Task<UserDto> GetUserByIdAsync(long id)
        {
            var user = await _userDbService.GetByIdOrThrowException(id);
            var userMembership = await _userMembershipDbService.GetActiveUserMembershipsAsync(user.Id);

            return new UserDto(user, userMembership);
        }

        public async Task<List<UserShortDto>> GetActiveUserShortListAsync()
        {
            var users = await _userDbService.GetActiveUsersAsync();

            return users.Select(u => new UserShortDto(u))
                .ToList();
        }

        public async Task<List<UserShortDto>> GetArchivedUsersAsync()
        {
            var users = await _userDbService.GetArchivedUsersAsync();

            return users.Select(u => new UserShortDto(u))
                .ToList();
        }

        public async Task<List<UserShortDto>> GetCoachStudentsByName(long coachId, string name)
        {
            var users = await _userMembershipDbService.GetCoachActiveStudentByName(coachId, name);
            return users.Select(u => new UserShortDto(u))
                .ToList();
        }

        public async Task<List<UserShortDto>> FindActiveUsersAsync(UserFilter filter)
        {
            var result = await _userDbService.GetActiveUserListAlphabetAscending(0, 100, filter);
            return result.Users
                .Select(user => new UserShortDto(user))
                .ToList();
        }

        public async Task<UsersDataDto> GetActiveUserShortListCutDataAsync(int startIndex, int finishIndex, UserFilter filter)
        {
            var pagedResult = await _userDbService.GetActiveUserListAlphabetAscending(startIndex, finishIndex, filter);
            var users = pagedResult.Users.Select(u => new UserDto(u))
                .ToList();

            foreach (var user in users)
            {
                var userMemberships = await _userMembershipDbService.GetActiveUserMembershipsAsync(user.Id.Value);
                user.UserMembershipDtos = userMemberships.Select(um => new UserMembershipDto(um)).ToList();
            }

            return new UsersDataDto
            {
                TotalCount = pagedResult.TotalCount,
                Users = users
            };
        }

        public async Task<long> CreateUserAsync(UserCreationDto userData)
        {
            UserEntity user = null;
            await EnsureUserCreateable(userData);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {       
                user = await _userDbService.CreateUser(userData);

                await _unitOfWork.SaveChangesAsync();

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

                        await _userMembershipApplicationService
                            .AddUserMembershipAsync(user.Id, userMembership);
                    }
                }

                if (user == null)
                {
                    throw new InvalidOperationException("Ошибка создания пользователя");
                }
            });

            return user.Id;
        }

        public async Task UpdateUserAsync(long userId, UserCreationDto userData)
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var user = await _userDbService.GetByIdOrThrowException(userId);
                await _userDbService.UpdateUser(userId, userData);

                await _userMembershipApplicationService.CloseExcessUserMemberships(userId, userData.UserMembershipDtos);

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
            });
        }

        public async Task CloseUserAsync(long id)
        {
            var user = await _userDbService.GetByIdOrThrowException(id);

            if (user.UserMemberships.Count() > 0)
            {
                throw new InvalidOperationException("Человек является участником одной или нескольких групп");
            }

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _userDbService.CloseAsync(id);
            });     
        }

        public async Task RecoverUserAsync(long id)
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _userDbService.RecoverAsync(id);
            });
            
        }

        public async Task DeleteUserAsync(long id)
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _userMembershipDbService.RemoveUserMemberships(id);

                await _unitOfWork.SaveChangesAsync(); 

                await _userDbService.Delete(id);
            });
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

        public async Task<List<UserSeminarHistoryItemDto>> GetUserSeminarHistory(long userId)
        {
            var seminars = await _seminarDbService.GetUserSeminarHistory(userId);

            return seminars.Select(s => new UserSeminarHistoryItemDto(s))
                .ToList();
        }

        public async Task<List<UserCertificationHistoryItemDto>> GetUserCertificationHistory(long userId)
        {
            var members = await _seminarDbService.GetUserCertificationHistory(userId);

            return members.Select(sm => new UserCertificationHistoryItemDto(sm)).ToList();
        }

        private async Task EnsureUserCreateable(UserCreationDto user)
        {
            var passwordMinLength = 1;

            if (await _userDbService.LoginExists(user.Login))
            {
                throw new InvalidOperationException("Логин занят");
            }
            if (user.Password.Length < passwordMinLength)
            {
                throw new InvalidOperationException($"Пароль должен содержать минимум {passwordMinLength} символов");
            }
            if (user.FirstName == "" || user.LastName == "")
            {
                throw new InvalidOperationException("Не указано фамилия или имя");
            }
        }

        private async Task EnsureUserExists(long userId)
        {
            if (!await _userDbService.ExistsActive(userId))
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