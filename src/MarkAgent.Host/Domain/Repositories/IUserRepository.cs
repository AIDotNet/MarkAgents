using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Domain.Repositories;

/// <summary>
/// 用户仓储接口
/// </summary>
public interface IUserRepository : IRepository<User, Guid>
{
    /// <summary>
    /// 根据邮箱获取用户
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据用户名获取用户
    /// </summary>
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据密码重置令牌获取用户
    /// </summary>
    Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据邮箱确认令牌获取用户
    /// </summary>
    Task<User?> GetByEmailConfirmationTokenAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 检查邮箱是否已存在
    /// </summary>
    Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 检查用户名是否已存在
    /// </summary>
    Task<bool> IsUserNameExistsAsync(string userName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取活跃用户数量
    /// </summary>
    Task<int> GetActiveUsersCountAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取指定日期范围内的新用户数量
    /// </summary>
    Task<int> GetNewUsersCountAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 更新用户最后登录时间
    /// </summary>
    Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default);
} 