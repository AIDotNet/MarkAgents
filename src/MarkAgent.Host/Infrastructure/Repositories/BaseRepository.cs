using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Infrastructure.Data;

namespace MarkAgent.Host.Infrastructure.Repositories;

/// <summary>
/// 基础仓储实现类
/// </summary>
/// <typeparam name="T">实体类型</typeparam>
/// <typeparam name="TKey"></typeparam>
public class BaseRepository<T, TKey> : IRepository<T, TKey> where T : BaseEntity<TKey> where TKey : notnull
{
    protected readonly MarkAgentDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(MarkAgentDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    public virtual async Task<T?> GetAsync(Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<List<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<(List<T> Items, int TotalCount)> GetPagedListAsync(
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        int pageIndex = 0,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (orderBy != null)
        {
            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        }
        else
        {
            query = query.OrderBy(e => e.CreatedAt);
        }

        var items = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.CountAsync(cancellationToken);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        var entry = await _dbSet.AddAsync(entity, cancellationToken);
        return entry.Entity;
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var entry = _dbSet.Update(entity);
        return Task.FromResult(entry.Entity);
    }

    public virtual Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        _dbSet.UpdateRange(entities);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 使用EF Core的批量更新API - 更高效的批量更新
    /// </summary>
    public virtual async Task<int> ExecuteUpdateAsync(
        Expression<Func<T, bool>> predicate,
        Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setPropertyCalls,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(predicate)
            .ExecuteUpdateAsync(setPropertyCalls, cancellationToken);
    }

    /// <summary>
    /// 使用EF Core的批量删除API - 更高效的批量删除
    /// </summary>
    public virtual async Task<int> ExecuteDeleteAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(predicate)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        // 软删除在DbContext的SaveChangesAsync中处理
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        await _dbSet.Where(x => x.Id.Equals(id))
            .ExecuteDeleteAsync(cancellationToken);
    }

    public virtual Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        _dbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 批量软删除 - 使用EF Core批量更新API
    /// </summary>
    public virtual async Task<int> SoftDeleteBatchAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteUpdateAsync(
            predicate,
            setters => setters
                .SetProperty(e => e.IsDeleted, true)
                .SetProperty(e => e.DeletedAt, DateTime.UtcNow)
                .SetProperty(e => e.UpdatedAt, DateTime.UtcNow),
            cancellationToken);
    }

    public virtual Task HardDeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        // 忽略全局查询过滤器进行硬删除
        _context.Entry(entity).State = EntityState.Deleted;
        return Task.CompletedTask;
    }

    public Task HardDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public virtual async Task HardDeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
        if (entity != null)
        {
            await HardDeleteAsync(entity, cancellationToken);
        }
    }

    /// <summary>
    /// 批量硬删除 - 使用EF Core批量删除API
    /// </summary>
    public virtual async Task<int> HardDeleteBatchAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .IgnoreQueryFilters()
            .Where(predicate)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}