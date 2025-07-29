using System.Linq.Expressions;
using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Domain.Repositories;

/// <summary>
/// 基础仓储接口
/// </summary>
/// <typeparam name="T">实体类型</typeparam>
/// <typeparam name="TKey"></typeparam>
public interface IRepository<T, in TKey> where T : BaseEntity<TKey> where TKey : notnull
{
    /// <summary>
    /// 根据ID获取实体
    /// </summary>
    Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据条件获取单个实体
    /// </summary>
    Task<T?> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据条件获取实体列表
    /// </summary>
    Task<List<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页获取实体列表
    /// </summary>
    Task<(List<T> Items, int TotalCount)> GetPagedListAsync(
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        int pageIndex = 0,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查实体是否存在
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取实体数量
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加实体
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量添加实体
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新实体
    /// </summary>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量更新实体
    /// </summary>
    Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除实体（软删除）
    /// </summary>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID删除实体（软删除）
    /// </summary>
    Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量删除实体（软删除）
    /// </summary>
    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// 硬删除实体
    /// </summary>
    Task HardDeleteAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID硬删除实体
    /// </summary>
    Task HardDeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 保存更改
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}