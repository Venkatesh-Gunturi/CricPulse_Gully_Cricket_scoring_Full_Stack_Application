namespace CricPulse.Application.Interfaces.Repositories;

public interface ITransactionManager
{
    Task BeginTransactionAsync();

    Task CommitTransactionAsync();

    Task RollbackTransactionAsync();
}