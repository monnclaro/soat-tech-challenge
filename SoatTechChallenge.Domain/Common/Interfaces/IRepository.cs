namespace SoatTechChallenge.Domain.Common.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    Task InsertAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(Guid id);
    IQueryable<TEntity> GetQueryable();
}