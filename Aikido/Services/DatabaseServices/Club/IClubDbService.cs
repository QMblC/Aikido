using Aikido.Dto.Clubs;
using Aikido.Entities;
using Aikido.Entities.Clubs;
using Aikido.Entities.Users;
using Aikido.Services.DatabaseServices.Base;

namespace Aikido.Services.DatabaseServices.Club
{
    public interface IClubDbService
    {
        Task<ClubEntity> GetByIdOrThrowException(long id);
        Task<ClubEntity> GetClubById(long id);
        Task<bool> ExistsActive(long id);
        Task<List<ClubEntity>> GetAllActiveAsync();
        Task<List<ClubEntity>> GetManagerClubs(long managerId);
        Task<long> CreateAsync(IClubDto clubData);
        Task UpdateAsync(long id, IClubDto clubData);
        Task UpdateAsync(ClubEntity club);
        Task CloseAsync(long id);
        Task RecoverAsync(long id);
        Task DeleteAsync(long id);
        Task<List<UserMembershipEntity>> GetClubMembersAsync(long clubId);
        Task RemoveAllMembersFromClubAsync(long clubId);
        Task<int> GetClubActiveMemberCountAsync(long clubId);
        Task<List<GroupEntity>> GetClubGroupsAsync(long clubId);
    }
}