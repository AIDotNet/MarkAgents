using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarkAgent.Host.Infrastructure.Repositories;

/// <summary>
/// 邮箱验证码仓储实现
/// </summary>
public class EmailVerificationCodeRepository : BaseRepository<EmailVerificationCode,Guid>, IEmailVerificationCodeRepository
{
    public EmailVerificationCodeRepository(MarkAgentDbContext context) : base(context)
    {
    }

    /// <summary>
    /// 根据邮箱和类型获取有效的验证码
    /// </summary>
    public async Task<EmailVerificationCode?> GetValidCodeAsync(string email, EmailVerificationCodeType type, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(x => x.Email == email 
                       && x.Type == type 
                       && !x.IsUsed 
                       && x.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 根据验证码获取有效的验证码
    /// </summary>
    public async Task<EmailVerificationCode?> GetValidCodeByCodeAsync(string code, EmailVerificationCodeType type, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(x => x.Code == code 
                       && x.Type == type 
                       && !x.IsUsed 
                       && x.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 清理过期的验证码
    /// </summary>
    public async Task CleanupExpiredCodesAsync(CancellationToken cancellationToken = default)
    {
        var expiredCodes = await _dbSet
            .Where(x => x.ExpiresAt <= DateTime.UtcNow || x.IsUsed)
            .ToListAsync(cancellationToken);

        if (expiredCodes.Any())
        {
            _dbSet.RemoveRange(expiredCodes);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
} 