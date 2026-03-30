using Aikido.AdditionalData.Enums;
using Aikido.Dto.Users;
using Aikido.Dto.Users.Creation;
using Aikido.Entities.Users;
using Aikido.Exceptions;
using Aikido.Services.DatabaseServices.Group;
using Aikido.Services.DatabaseServices.User;
using Aikido.Services.UnitOfWork;

namespace Aikido.Services.ApplicationServices
{
    public class UserMembershipApplicationService
    {
        private readonly IUserDbService _userDbService;
        private readonly IUserMembershipDbService _userMembershipDbService;
        private readonly IGroupDbService _groupDbService;
        private readonly IUnitOfWork _unitOfWork;

        public UserMembershipApplicationService(IUserDbService userDbService,
            IUserMembershipDbService userMembershipDbService,
            IGroupDbService groupDbService,
            IUnitOfWork unitOfWork)
        {
            _userDbService = userDbService;
            _userMembershipDbService = userMembershipDbService;
            _groupDbService = groupDbService;
            _unitOfWork = unitOfWork;
        }

        public async Task AddUserMembershipAsync(long userId, UserMembershipCreationDto dto)
        {
            await EnsureUserExists(userId);
            await EnsureGroupExists(dto.GroupId.Value);

            var userMembershipExists = await _userMembershipDbService.UserMembershipExists(userId,
                dto.GroupId.Value);

            if (!userMembershipExists)
            {
                await CreateUserMembershipAsync(userId, dto);
            }
            else if (IsRoleChanged(_userMembershipDbService.GetActiveUserMembership(userId, dto.GroupId.Value), dto))
            {
                await RecreateUserMembershipAsync(userId, dto);
            }
            else
            {
                await UpdateUserMembershipAsync(userId, dto); 
            }

            var mainUserMembership = _userMembershipDbService.GetMainUserMembership(userId);
            if (mainUserMembership == null)
            {
                await SetNewMainUserMembershipAsync(userId);
            }
        }

        private async Task CreateUserMembershipAsync(long userId, UserMembershipCreationDto dto)
        {
            
            var userMemberships = await _userMembershipDbService.GetActiveUserMembershipsAsUserAsync(userId);

            if (dto.IsMain && dto.RoleInGroup == EnumParser.ConvertEnumToString(Role.User))
            {
                await UnsetMainUserMembershipAsync(userId);
                await _userMembershipDbService.CreateUserMembershipAsync(userId, dto);
                await _unitOfWork.SaveChangesAsync();
                await SetNewMainUserMembershipAsync(userId, dto.GroupId.Value);
            }
            else if (dto.RoleInGroup == EnumParser.ConvertEnumToString(Role.Coach))
            {
                dto.IsMain = false;
                await _userMembershipDbService.CreateUserMembershipAsync(userId, dto);
                await _unitOfWork.SaveChangesAsync();
            }
            else if (dto.IsMain == false && userMemberships.Count == 0)
            {
                dto.IsMain = true;
                await _userMembershipDbService.CreateUserMembershipAsync(userId, dto);
                await _unitOfWork.SaveChangesAsync();
                await SetNewMainUserMembershipAsync(userId, dto.GroupId.Value);
            }
            else
            {
                await _userMembershipDbService.CreateUserMembershipAsync(userId, dto);
                await _unitOfWork.SaveChangesAsync();
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
            var existing = _userMembershipDbService.GetActiveUserMembership(userId, dto.GroupId.Value);

            if (existing.IsMain == true && dto.IsMain == false)
            {
                await UnsetMainUserMembershipAsync(userId);
                await _unitOfWork.SaveChangesAsync();
                await SetNewMainUserMembershipAsync(userId);
            }
            else if (existing.IsMain == false && dto.IsMain == true)
            {
                await UnsetMainUserMembershipAsync(userId);
                await _unitOfWork.SaveChangesAsync();
                await SetNewMainUserMembershipAsync(userId, dto.GroupId.Value);
            }
            await _userMembershipDbService.UpdateUserMembershipAsync(userId, dto);
        }

        public async Task CloseUserMembershipAsync(long userId, long groupId)
        {
            var userMembership = _userMembershipDbService.GetActiveUserMembership(userId, groupId);

            if (userMembership.IsMain)
            {
                await UnsetMainUserMembershipAsync(userId);
                await _userMembershipDbService.CloseUserMembershipAsync(userId, groupId);
                await _unitOfWork.SaveChangesAsync();
                await SetNewMainUserMembershipAsync(userId);
            }
            else
            {
                await _userMembershipDbService.CloseUserMembershipAsync(userId, groupId);
            }   
        }

        public async Task CloseExcessUserMemberships(long userId, List<UserMembershipCreationDto> newMemberships)
        {
            var oldUserMemberships = await _userMembershipDbService.GetActiveUserMembershipsAsync(userId);

            var excessUserMemberships = oldUserMemberships.Where(um =>
                !newMemberships.Any(newUM => um.GroupId == newUM.GroupId))
                .ToList();

            await _userMembershipDbService.CloseUserMembershipsAsync(excessUserMemberships.Select(um => um.Id).ToList());
            if (excessUserMemberships.Any(um => um.IsMain))
            {
                await SetNewMainUserMembershipAsync(userId);
            }
        }

        private async Task SetNewMainUserMembershipAsync(long userId)
        {
            var userMemberships = await _userMembershipDbService.GetActiveUserMembershipsAsUserAsync(userId);
            if (userMemberships.Where(um => um.RoleInGroup == Role.User).Count() > 0)
            {
                var newMainUserMembership = userMemberships.First(um => um.RoleInGroup == Role.User);
                newMainUserMembership.IsMain = true;
                await _userMembershipDbService.UpdateUserMembershipAsync(newMainUserMembership);

                if (newMainUserMembership?.User != null)
                {
                    newMainUserMembership.User.MainUserMembershipAsUserId = newMainUserMembership.Id;
                }
                await _userDbService.UpdateUser(newMainUserMembership.User);
            }
            
        }

        private async Task SetNewMainUserMembershipAsync(long userId, long groupId)
        {
            var userMembership = _userMembershipDbService.GetActiveUserMembership(userId, groupId);
            if (userMembership?.User != null)
            {
                userMembership.User.MainUserMembershipAsUserId = userMembership.Id;
                await _userDbService.UpdateUser(userMembership.User);
            }
        }

        private async Task UnsetMainUserMembershipAsync(long userId)
        {
            var mainUserMembership = _userMembershipDbService.GetMainUserMembership(userId);
            if (mainUserMembership != null)
            {
                mainUserMembership.IsMain = false;
                await _userMembershipDbService.UpdateUserMembershipAsync(mainUserMembership);
                mainUserMembership.User.MainUserMembershipAsUserId = null;
                await _userDbService.UpdateUser(mainUserMembership.User);
            } 
        }

        public async Task RecoverUserMembershipAsync(long userId, long groupId)
        {
            await _userMembershipDbService.RecoverUserMembershipAsync(userId, groupId);
        }

        public async Task RemoveUserMembership(long userId, long groupId)
        {
            await _userMembershipDbService.RemoveUserMembershipAsync(userId, groupId);
        }


        public async Task<List<UserMembershipDto>> GetUserMembershipsAsync(long userId)
        {
            var userGroups = await _userMembershipDbService.GetActiveUserMembershipsAsync(userId);
            return userGroups.Select(um => new UserMembershipDto(um)).ToList();
        }

        private bool IsRoleChanged(UserMembershipEntity existing, UserMembershipCreationDto dto)
        {
            return existing.RoleInGroup != EnumParser.ConvertStringToEnum<Role>(dto.RoleInGroup);
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
