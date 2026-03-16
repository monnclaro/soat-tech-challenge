using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Domain.Common.Interfaces;

namespace SoatTechChallenge.Infrastucture.Persistence;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    } 

    public async Task InsertAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TEntity entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetAsync(id);

        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    private async Task<TEntity?> GetAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public IQueryable<TEntity> GetQueryable(bool asSplitQuery = false)
    {
        var dbSet = _dbSet;
        
        if (asSplitQuery)
        {
            dbSet.AsSplitQuery();
        }
        
        return dbSet;
    }
}