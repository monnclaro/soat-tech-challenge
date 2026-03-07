namespace SoatTechChallenge.Infrastructure.Interfaces;

public interface IRepository<T> where T : class
{
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Delete(T entity);
    IQueryable<T> Query();
    Task SaveChangesAsync(CancellationToken ct = default);
}