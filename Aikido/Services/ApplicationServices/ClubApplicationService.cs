using Aikido.Dto;
using Aikido.Services.DatabaseServices.Club;
using Aikido.Services.DatabaseServices.Group;
using Aikido.Services.DatabaseServices.User;
using Aikido.Exceptions;
using Aikido.Entities;
using Aikido.Entities.Users;
using Aikido.AdditionalData;
using Aikido.Dto.Users;
using Aikido.Dto.Groups;

namespace Aikido.Application.Services
{
    public class ClubApplicationService
    {
        private readonly IClubDbService _clubDbService;
        private readonly IGroupDbService _groupDbService;
        private readonly IUserDbService _userDbService;

        public ClubApplicationService(
            IClubDbService clubDbService,
            IGroupDbService groupDbService,
            IUserDbService userDbService)
        {
            _clubDbService = clubDbService;
            _groupDbService = groupDbService;
            _userDbService = userDbService;
        }

        public async Task<ClubDto> GetClubByIdAsync(long id)
        {
            var club = await _clubDbService.GetByIdOrThrowException(id);
            return new ClubDto(club);
        }

        public async Task<List<ClubDto>> GetAllClubsAsync()
        {
            var clubs = await _clubDbService.GetAllAsync();
            return clubs.Select(c => new ClubDto(c)).ToList();
        }

        public async Task<List<GroupDto>> GetClubGroups(long clubId)
        {
            var groups = await _clubDbService.GetClubGroupsAsync(clubId);
            
            return groups.Select(c => new GroupDto(c)).
                ToList();
        }

        public async Task<List<ClubDto>> GetManagerClubsAsync(long managerId)
        {
            var clubs = await _clubDbService.GetManagerClubs(managerId);

            return clubs.Select(c => new ClubDto(c))
                .ToList();
        }

        public async Task<ClubDetailsDto> GetClubDetailsAsync(long id)
        {
            var club = await _clubDbService.GetByIdOrThrowException(id);
            var groups = await _groupDbService.GetGroupsByClub(id);
            var members = new List<UserMembershipEntity>();
            if (groups.Count > 0)
            {
                members = await _clubDbService.GetClubMembersAsync(id);
            }
            
            return new ClubDetailsDto(club, groups, members);
        }

        public async Task<long> CreateClubAsync(ClubDto clubData)
        {
            return await _clubDbService.CreateAsync(clubData);
        }

        public async Task UpdateClubAsync(long id, ClubDto clubData)
        {
            if (!await _clubDbService.Exists(id))
            {
                throw new EntityNotFoundException($"Клуб с Id = {id} не найден");
            }
            await _clubDbService.UpdateAsync(id, clubData);
        }

        public async Task DeleteClubAsync(long id)
        {
            if (!await _clubDbService.Exists(id))
            {
                throw new EntityNotFoundException($"Клуб с Id = {id} не найден");
            }

            await _clubDbService.RemoveAllMembersFromClubAsync(id);
            await _clubDbService.DeleteAsync(id);
        }

        public async Task<bool> ClubExistsAsync(long id)
        {
            return await _clubDbService.Exists(id);
        }

        public async Task<List<UserShortDto>> GetClubMembersAsync(long clubId, Role role = Role.User)
        {
            var members = await _clubDbService.GetClubMembersAsync(clubId);
            return members.Where(m => m.User != null && m.RoleInGroup == role)
                         .Select(m => new UserShortDto(m.User!))
                         .ToList();
        }
    }
}