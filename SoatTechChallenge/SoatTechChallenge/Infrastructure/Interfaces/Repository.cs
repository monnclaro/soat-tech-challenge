using SoatTechChallenge.Host.Common.Services;
using SoatTechChallenge.Infrastructure.Database;

namespace SoatTechChallenge.Infrastructure.Interfaces;

public class Repository<T> : IRepository<T>, ITransientService where T : class
{
    private readonly SoatTechChallengeDbContext _context;

    public Repository(SoatTechChallengeDbContext context)
    {
        _context = context;
    }
    
    public IQueryable<T> Query() => _context.Set<T>();
    
    public async Task AddAsync(T entity, CancellationToken ct = default) => await _context.Set<T>().AddAsync(entity, ct);
    public void Update(T entity) => _context.Set<T>().Update(entity);
    public void Delete(T entity) => _context.Set<T>().Remove(entity);

    public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}