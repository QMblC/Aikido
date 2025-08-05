using Aikido.Data;
using Aikido.Entities;
using Aikido.Exceptions;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services.DatabaseServices.Base
{
    public abstract class DbService<T, TService> : IDbService<T>
        where T : class, IDbEntity
        where TService : class
    {
        protected readonly AppDbContext context;
        protected readonly ILogger<TService> logger;

        public DbService(AppDbContext context, ILogger<TService> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<bool> Exists(long id) =>
            await context.Set<T>().AnyAsync(item => item.Id == id);

        public IQueryable<T> Query() => context.Set<T>().AsQueryable();

        public async Task<T> GetByIdOrThrowException(long id)
        {
            var entity = await TryGetById(id);
            if (entity == null)
            {
                var ex = new EntityNotFoundException(typeof(T).Name, id);
                logger.LogError(ex, "Ошибка получения {Entity} с Id = {Id}", typeof(T).Name, id);
                throw ex;
            }
            return entity;
        }

        public async Task<List<T>> GetByIdOrThrowException(List<long> ids)
        {
            var entities = await Task.WhenAll(ids.Select(id => TryGetById(id)));

            if (entities.Any(entity => entity == null))
            {
                var ex = new EntityNotFoundException(typeof(T).Name);
                logger.LogError(ex, "Ошибка получения {Entity}", typeof(T).Name);
                throw ex;
            }

            return entities.ToList();
        }

        public async Task<T?> TryGetById(long id) =>
            await context.Set<T>().FirstOrDefaultAsync(e => e.Id == id);

        public async Task<List<T>> GetAll() =>
            await context.Set<T>().ToListAsync();

        public async Task Create(T entity)
        {
            await context.Set<T>().AddAsync(entity);
            await SaveChangesAsync();
            logger.LogInformation("{Entity} создана", typeof(T).Name);
        }

        public async Task Create(List<T> entities)
        {
            await context.Set<T>().AddRangeAsync(entities);
            await SaveChangesAsync();
            logger.LogInformation("Список {Entity} создан", typeof(T).Name);
        }

        public async Task UpdateOrThrowException(T entity)
        {
            if (await Exists(entity.Id))
            {
                context.Set<T>().Update(entity);
                await SaveChangesAsync();
                logger.LogInformation("{Entity} обновлена", typeof(T).Name);
            }
            else
            {
                var ex = new EntityNotFoundException(typeof(T).Name, entity.Id);
                logger.LogError(ex, "Ошибка обновления {Entity}", typeof(T).Name);
                throw ex;
            }
        }

        public async Task UpdateOrThrowException(List<T> entities)
        {
            var existenceChecks = await Task.WhenAll(entities.Select(entity => Exists(entity.Id)));

            if (existenceChecks.All(x => x))
            {
                context.Set<T>().UpdateRange(entities);
                await SaveChangesAsync();
                logger.LogInformation("Список {Entity} обновлен", typeof(T).Name);
            }
            else
            {
                var ex = new EntityNotFoundException(typeof(T).Name);
                logger.LogError(ex, "Ошибка обновления списка {Entity}", typeof(T).Name);
                throw ex;
            }
        }

        public async Task DeleteById(long id)
        {
            var entity = await TryGetById(id);
            if (entity == null)
            {
                var ex = new EntityNotFoundException(typeof(T).Name, id);
                logger.LogError(ex, "Ошибка удаления {Entity}", typeof(T).Name);
                throw ex;
            }

            context.Set<T>().Remove(entity);

            await SaveChangesAsync();

            logger.LogInformation("{Entity} удалена", typeof(T).Name);
        }

        protected async Task SaveChangesAsync()
        {
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var saveEx = new SaveDatabaseException(typeof(T).Name, ex.InnerException?.Message ?? ex.Message);
                logger.LogError(saveEx, "Ошибка сохранения {Entity}", typeof(T).Name);
                throw saveEx;
            }
        }
    }
}
