using Aikido.AdditionalData.Enums;
using Aikido.Entities.Clubs;

namespace Aikido.Services.DatabaseServices.Club
{
    public interface IClubStaffDbService
    {
        Task<List<ClubStaffEntity>> GetClubStaffByUser(long userId);
        Task<List<ClubStaffEntity>> GetClubStaffByClub(long clubId);
        Task<ClubStaffEntity> GetClubStaff(long clubId, long userId);
        Task CreateRangeAsync(long clubId, List<long> userIds);
        Task DeleteRangeAsync(long clubId, List<long> userIds);
        Task CreateAsync(long clubId, long userId, Role roleInClub = Role.Coach);
        Task DeleteAsync(long clubId, long userId);
    }
}
