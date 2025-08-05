namespace Aikido.Services.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task ExecuteInTransactionAsync(Func<Task> action);
    }
}
