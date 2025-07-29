using Microsoft.EntityFrameworkCore;
using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Infrastructure.Data;

namespace MarkAgent.Host.Infrastructure.Repositories;

/// <summary>
/// 用户仓储实现类
/// </summary>
public class UserRepository : BaseRepository<User, Guid>, IUserRepository
{
    public UserRepository(MarkAgentDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserKeys)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserKeys)
            .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
    }

    public async Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(
            u => u.PasswordResetToken == token, 
            cancellationToken);
    }

    public async Task<User?> GetByEmailConfirmationTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(
            u => u.EmailConfirmationToken == token, 
            cancellationToken);
    }

    public async Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> IsUserNameExistsAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(u => u.UserName == userName, cancellationToken);
    }

    public async Task<int> GetActiveUsersCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(u => u.Status == UserStatus.Active, cancellationToken);
    }

    public async Task<int> GetNewUsersCountAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(
            u => u.CreatedAt >= startDate && u.CreatedAt <= endDate, 
            cancellationToken);
    }

    public async Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(userId, cancellationToken);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await UpdateAsync(user, cancellationToken);
        }
    }
} 