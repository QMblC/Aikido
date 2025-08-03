using Aikido.Entities;

namespace Aikido.Services.Base
{
    public interface IDbService<T> where T : class, IDbEntity
    {
        Task<bool> Exists(long id);
        IQueryable<T> Query();
        Task<T?> TryGetById(long id);
        Task<List<T>> GetAll();
        Task Create(T entity);
        Task UpdateOrThrowException(T entity);
        Task DeleteById(long id);
    }

}
