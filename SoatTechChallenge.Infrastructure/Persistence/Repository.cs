using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Domain.Common.Interfaces;
using SoatTechChallenge.Infrastucture.Database;

namespace SoatTechChallenge.Infrastucture.Persistence;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly SoatTechChallengeDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository(SoatTechChallengeDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public async Task InsertAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public Task UpdateAsync(TEntity entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TEntity entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public IQueryable<TEntity> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}