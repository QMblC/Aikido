using Aikido.Data;

namespace Aikido.Services
{
    public abstract class DbService
    {
        internal AppDbContext context;

        public DbService(AppDbContext context)
        {
            this.context = context;
        }

        internal async Task SaveDb()
        {
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при обработке: {ex.InnerException?.Message}", ex);
            }
        }

        internal async Task<long> Create<T>(T entity)
        {
            var savedEntity = context.Add(entity);

            await SaveDb();

            return 1;
        }
    }
}
