using System.Data.Common;

namespace SoatTechChallenge.Domain.Common.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    Task InsertAsync(TEntity entity);
    Task UpdateAsync(TEntity entity, bool autosave = true);
    Task DeleteAsync(Guid id);
    IQueryable<TEntity> GetQueryable();
    IQueryable<TEntity> GetRawSql(string query);
}