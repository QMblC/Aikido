namespace Aikido.Services.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync();
        Task ExecuteInTransactionAsync(Func<Task> action);
    }
}
