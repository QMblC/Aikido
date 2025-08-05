
using Aikido.Data;

namespace Aikido.Services.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly ILogger _logger;

        public UnitOfWork(AppDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await action(); 
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Транзакция успешно завершена");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка в транзакции, выполнен откат");
                throw;
            }
        }
    }
}
