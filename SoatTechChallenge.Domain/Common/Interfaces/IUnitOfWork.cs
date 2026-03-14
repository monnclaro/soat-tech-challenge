namespace SoatTechChallenge.Domain.Common.Interfaces;

public interface IUnitOfWork
{
    Task ExecuteInTransactionAsync(Func<Task> action);
}