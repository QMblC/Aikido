using Aikido.Entities;

namespace Aikido.Services.DatabaseServices.Base
{
    public interface IDbService<T> where T : class, IDbEntity
    {
        Task<bool> Exists(long id);
        IQueryable<T> Query();
        Task<T> GetByIdOrThrowException(long id);
        Task<T?> TryGetById(long id);
        Task<List<T>> GetAll();
        Task Create(T entity);
        Task Create(List<T> entities);
        Task UpdateOrThrowException(T entity);
        Task UpdateOrThrowException(List<T> entities);
        Task DeleteById(long id);
    }

}
