using Aikido.Entities;
using Aikido.Services.DatabaseServices.Base;

namespace Aikido.Services.DatabaseServices.Club
{
    public interface IClubDbService : IDbService<ClubEntity>
    {
        List<GroupEntity> GetClubGroups(long clubId);
    }
}
