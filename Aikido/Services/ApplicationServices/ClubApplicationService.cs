using Aikido.AdditionalData.Enums;
using Aikido.Dto;
using Aikido.Dto.Groups;
using Aikido.Dto.Users;
using Aikido.Entities;
using Aikido.Entities.Users;
using Aikido.Exceptions;
using Aikido.Services;
using Aikido.Services.DatabaseServices.Club;
using Aikido.Services.DatabaseServices.Group;
using Aikido.Services.DatabaseServices.User;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Aikido.Application.Services
{
    public class ClubApplicationService
    {
        private readonly IClubDbService _clubDbService;
        private readonly IGroupDbService _groupDbService;
        private readonly IUserDbService _userDbService;
        private readonly ScheduleDbService _scheduleDbService;
        private readonly IClubStaffDbService _clubStaffDbService;

        public ClubApplicationService(
            IClubDbService clubDbService,
            IGroupDbService groupDbService,
            IUserDbService userDbService,
            ScheduleDbService scheduleDbService,
            IClubStaffDbService clubStaffDbService)
        {
            _clubDbService = clubDbService;
            _groupDbService = groupDbService;
            _userDbService = userDbService;
            _scheduleDbService = scheduleDbService;
            _clubStaffDbService = clubStaffDbService;
        }

        public async Task<ClubDto> GetClubByIdAsync(long id)
        {
            var club = await _clubDbService.GetByIdOrThrowException(id);
            return new ClubDto(club);
        }

        public async Task<List<ClubDto>> GetAllClubsAsync()
        {
            var clubs = await _clubDbService.GetAllActiveAsync();
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
            var club = await _clubDbService.CreateAsync(clubData);
            if (clubData.ManagerId != null)
            {
                await _clubStaffDbService.CreateAsync(club, clubData.ManagerId.Value, true);
            }
            
            return club;
        }

        public async Task UpdateClubAsync(long id, ClubDto clubData)
        {
            if (!await _clubDbService.ExistsActive(id))
            {
                throw new EntityNotFoundException($"Клуб с Id = {id} не найден");
            }
            await _clubDbService.UpdateAsync(id, clubData);
            var club = await _clubDbService.GetClubById(id);

            if (club.ManagerId != clubData.ManagerId)
            {
                if (club.ManagerId != null)
                {
                    await _clubStaffDbService.CreateAsync(id, club.ManagerId.Value, true);
                }
                if (clubData.ManagerId != null)
                {
                    await _clubStaffDbService.DeleteAsync(id, clubData.ManagerId.Value);
                }        
            }

        }

        public async Task CloseClubAsync(long id)
        {      
            var groups = await _clubDbService.GetClubGroupsAsync(id);

            if (groups.Count == 0)
            {
                await _clubDbService.CloseAsync(id);
            }
            else
            {
                throw new InvalidOperationException("Невозможно закрыть клуб, пока в нём есть активные группы");
            } 
        }

        public async Task RecoverClubAsync(long id)
        {
            await _clubDbService.RecoverAsync(id);
        }

        public async Task DeleteClubAsync(long id)
        {
            if (!await _clubDbService.ExistsActive(id))
            {
                throw new EntityNotFoundException($"Клуб с Id = {id} не найден");
            }

            await _clubDbService.RemoveAllMembersFromClubAsync(id);
            var club = await _clubDbService.GetClubById(id);
            if (club.ManagerId != null)
            {
                await _clubStaffDbService.DeleteAsync(id, club.ManagerId.Value);
            }
            
            await _clubDbService.DeleteAsync(id);
        }

        public async Task<bool> ClubExistsAsync(long id)
        {
            return await _clubDbService.ExistsActive(id);
        }

        public async Task<List<UserShortDto>> GetClubMembersAsync(long clubId, Role role = Role.User)
        {
            var members = await _clubDbService.GetClubMembersAsync(clubId);
            return members.Where(m => m.User != null && m.RoleInGroup == role)
                         .Select(m => new UserShortDto(m.User!))
                         .ToList();
        }

        public async Task<List<UserShortDto>> GetClubStaff(long clubId)
        {
            var staff = await _clubStaffDbService.GetClubStaffByClub(clubId);

            return staff.Select(cs => new UserShortDto(cs.User))
                .ToList();
        }

        public async Task UpdateClubStaff(long clubId, List<long> newStaff)
        {
            var oldStaff = await _clubStaffDbService.GetClubStaffByClub(clubId);
            var staffToCreate = newStaff.Where(id => !oldStaff.Any(cs => cs.UserId == id))
                .ToList();
            var staffToDelete = oldStaff.Where(cs => !newStaff.Any(id =>  cs.UserId == id))
                .Select(cs => cs.UserId)
                .ToList();

            await _clubStaffDbService.DeleteRangeAsync(clubId, staffToDelete);
            await _clubStaffDbService.CreateRangeAsync(clubId, staffToCreate);
        }
    }
}