using Aikido.Entities.Clubs;

namespace Aikido.Services.DatabaseServices.Club
{
    public interface IClubStaffDbService
    {
        public Task<List<ClubStaffEntity>> GetClubStaffByUser(long userId);
        public Task<List<ClubStaffEntity>> GetClubStaffByClub(long clubId);
        public Task<ClubStaffEntity> GetClubStaff(long clubId, long userId);
        public Task CreateRangeAsync(long clubId, List<long> userIds);
        public Task DeleteRangeAsync(long clubId, List<long> userIds);
    }
}
