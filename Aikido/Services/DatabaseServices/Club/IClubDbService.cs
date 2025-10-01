using Aikido.Dto;
using Aikido.Entities;
using Aikido.Services.DatabaseServices.Base;

namespace Aikido.Services.DatabaseServices.Club
{
    public interface IClubDbService
    {
        Task<ClubEntity> GetByIdOrThrowException(long id);
        Task<ClubEntity> GetClubById(long id);
        Task<bool> Exists(long id);
        Task<List<ClubEntity>> GetAllAsync();
        Task<long> CreateAsync(ClubDto clubData);
        Task UpdateAsync(long id, ClubDto clubData);
        Task DeleteAsync(long id);

        // Новые методы для работы с участниками клуба
        Task<List<UserClub>> GetClubMembersAsync(long clubId);
        Task RemoveAllMembersFromClubAsync(long clubId);
        Task<int> GetClubMemberCountAsync(long clubId);
        Task<List<GroupEntity>> GetClubGroupsAsync(long clubId);
    }
}