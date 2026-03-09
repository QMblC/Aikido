using Aikido.AdditionalData.Enums;
using Aikido.Dto.Users;
using Aikido.Dto.Users.Creation;
using Aikido.Entities.Users;
using Aikido.Exceptions;
using Aikido.Services.DatabaseServices.Group;
using Aikido.Services.DatabaseServices.User;

namespace Aikido.Services.ApplicationServices
{
    public class UserMembershipApplicationService
    {
        private readonly IUserDbService _userDbService;
        private readonly IGroupDbService _groupDbService;

        public UserMembershipApplicationService(IUserDbService userDbService,
            IGroupDbService groupDbService)
        {
            _userDbService = userDbService;
            _groupDbService = groupDbService;
        }

        public async Task AddUserMembershipAsync(long userId, UserMembershipCreationDto dto)
        {
            await EnsureUserExists(userId);
            await EnsureGroupExists(dto.GroupId.Value);

            var userMembershipExists = await _userDbService.UserMembershipExists(userId,
                dto.GroupId.Value);

            if (!userMembershipExists)
            {
                await CreateUserMembershipAsync(userId, dto);
            }
            else if (IsRoleChanged(_userDbService.GetActiveUserMembership(userId, dto.GroupId.Value), dto))
            {
                await RecreateUserMembershipAsync(userId, dto);
            }
            else
            {
                await UpdateUserMembershipAsync(userId, dto); 
            }
        }

        private async Task CreateUserMembershipAsync(long userId, UserMembershipCreationDto dto)
        {
            
            var userMemberships = await _userDbService.GetActiveUserMembershipsAsUserAsync(userId);

            if (dto.IsMain && dto.RoleInGroup == EnumParser.ConvertEnumToString(Role.User))
            {
                await UnsetMainUserMembershipAsync(userId);
                await _userDbService.CreateUserMembershipAsync(userId, dto);
                await SetNewMainUserMembershipAsync(userId, dto.GroupId.Value);
            }
            else if (dto.IsMain && dto.RoleInGroup == EnumParser.ConvertEnumToString(Role.Coach))
            {
                dto.IsMain = false;
                await _userDbService.CreateUserMembershipAsync(userId, dto);
            }
            else if (dto.IsMain == false && userMemberships.Count == 0)
            {
                dto.IsMain = true;
                await _userDbService.CreateUserMembershipAsync(userId, dto);
                await SetNewMainUserMembershipAsync(userId, dto.GroupId.Value);
            }
            else
            {
                await _userDbService.CreateUserMembershipAsync(userId, dto);
            }
        }

        private async Task RecreateUserMembershipAsync(long userId, UserMembershipCreationDto dto)
        {
            await CloseUserMembershipAsync(userId, dto.GroupId.Value);
            await CreateUserMembershipAsync(userId, dto);

            if (dto.IsMain && dto.RoleInGroup == EnumParser.ConvertEnumToString(Role.User))
            {
                await SetNewMainUserMembershipAsync(userId, dto.GroupId.Value);
            }    
        }

        private async Task UpdateUserMembershipAsync(long userId, UserMembershipCreationDto dto)
        {
            var existing = _userDbService.GetActiveUserMembership(userId, dto.GroupId.Value);

            if (existing.IsMain == true && dto.IsMain == false)
            {
                await UnsetMainUserMembershipAsync(userId);
                await SetNewMainUserMembershipAsync(userId);
            }
            else if (existing.IsMain == false && dto.IsMain == true)
            {
                await UnsetMainUserMembershipAsync(userId);
                await SetNewMainUserMembershipAsync(userId, dto.GroupId.Value);
            }
            await _userDbService.UpdateUserMembershipAsync(userId, dto);
        }

        public async Task CloseUserMembershipAsync(long userId, long groupId)
        {
            var userMembership = _userDbService.GetActiveUserMembership(userId, groupId);

            if (userMembership.IsMain)
            {
                await UnsetMainUserMembershipAsync(userId);
                await _userDbService.CloseUserMembershipAsync(userId, groupId);
                await SetNewMainUserMembershipAsync(userId);
            }
            else
            {
                await _userDbService.CloseUserMembershipAsync(userId, groupId);
            }   
        }

        private async Task SetNewMainUserMembershipAsync(long userId)
        {
            var userMemberships = await _userDbService.GetActiveUserMembershipsAsUserAsync(userId);
            if (userMemberships.Where(um => um.RoleInGroup == Role.User).Count() > 0)
            {
                var newMainUserMembership = userMemberships.First(um => um.RoleInGroup == Role.User);
                newMainUserMembership.IsMain = true;
                await _userDbService.UpdateUserMembershipAsync(newMainUserMembership);

                if (newMainUserMembership?.User != null)
                {
                    newMainUserMembership.User.MainUserMembershipAsUserId = newMainUserMembership.Id;
                }
                await _userDbService.UpdateUser(newMainUserMembership.User);
            }
            
        }

        private async Task SetNewMainUserMembershipAsync(long userId, long groupId)
        {
            var userMembership = _userDbService.GetActiveUserMembership(userId, groupId);
            if (userMembership?.User != null)
            {
                userMembership.User.MainUserMembershipAsUserId = userMembership.Id;
                await _userDbService.UpdateUser(userMembership.User);
            }
        }

        private async Task UnsetMainUserMembershipAsync(long userId)
        {
            var mainUserMembership = _userDbService.GetMainUserMembership(userId);
            if (mainUserMembership != null)
            {
                mainUserMembership.IsMain = false;
                await _userDbService.UpdateUserMembershipAsync(mainUserMembership);
                mainUserMembership.User.MainUserMembershipAsUserId = null;
                await _userDbService.UpdateUser(mainUserMembership.User);
            } 
        }

        public async Task RecoverUserMembershipAsync(long userId, long groupId)
        {
            await _userDbService.RecoverUserMembershipAsync(userId, groupId);
        }

        public async Task RemoveUserMembership(long userId, long groupId)
        {
            await _userDbService.RemoveUserMembershipAsync(userId, groupId);
        }

        public async Task<List<UserMembershipDto>> GetUserMembershipsAsync(long userId)
        {
            var userGroups = await _userDbService.GetActiveUserMembershipsAsync(userId);
            return userGroups.Select(um => new UserMembershipDto(um)).ToList();
        }

        private bool IsRoleChanged(UserMembershipEntity existing, UserMembershipCreationDto dto)
        {
            return existing.RoleInGroup != EnumParser.ConvertStringToEnum<Role>(dto.RoleInGroup);
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
