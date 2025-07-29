using BCrypt.Net;
using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using System.Security.Cryptography;

namespace MarkAgent.Host.Services;

/// <summary>
/// 认证服务实现
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserKeyRepository _userKeyRepository;
    private readonly IUserKeyService _userKeyService;
    private readonly JwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IUserKeyRepository userKeyRepository,
        IUserKeyService userKeyService,
        JwtService jwtService,
        IEmailService emailService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _userKeyRepository = userKeyRepository;
        _userKeyService = userKeyService;
        _jwtService = jwtService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, User? User)> RegisterAsync(string email, string userName, string password, string verificationCode, CancellationToken cancellationToken = default)
    {
        try
        {
            // 验证邮箱验证码
            var isCodeValid = await _emailService.VerifyCodeAsync(email, verificationCode, EmailVerificationCodeType.Registration, cancellationToken);
            if (!isCodeValid)
            {
                return (false, "邮箱验证码无效或已过期", null);
            }

            // 检查邮箱是否已存在
            if (await _userRepository.IsEmailExistsAsync(email, cancellationToken))
            {
                return (false, "邮箱已存在", null);
            }

            // 检查用户名是否已存在
            if (await _userRepository.IsUserNameExistsAsync(userName, cancellationToken))
            {
                return (false, "用户名已存在", null);
            }

            // 创建用户
            var user = new User
            {
                Email = email,
                UserName = userName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                EmailConfirmationToken = GenerateToken(),
                Status = UserStatus.Active
            };

            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            // 为新用户创建默认密钥
            var keyResult = await _userKeyService.CreateKeyAsync(user.Id, "默认密钥", "系统自动创建的默认密钥", cancellationToken);
            if (keyResult.Success && keyResult.UserKey != null)
            {
                await _userKeyService.SetDefaultKeyAsync(user.Id, keyResult.UserKey.Id, cancellationToken);
            }

            _logger.LogInformation("用户注册成功: {Email}", email);
            return (true, "注册成功", user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用户注册失败: {Email}", email);
            return (false, "注册失败", null);
        }
    }

    public async Task<(bool Success, string Message, string? Token, User? User)> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                return (false, "邮箱或密码错误", null, null);
            }

            if (user.Status != UserStatus.Active)
            {
                return (false, "账户已被禁用", null, null);
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return (false, "邮箱或密码错误", null, null);
            }

            // 更新最后登录时间
            await _userRepository.UpdateLastLoginAsync(user.Id, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            // 生成JWT令牌
            var token = _jwtService.GenerateToken(user);

            _logger.LogInformation("用户登录成功: {Email}", email);
            return (true, "登录成功", token, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用户登录失败: {Email}", email);
            return (false, "登录失败", null, null);
        }
    }

    public async Task<(bool Valid, User? User)> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _jwtService.GetUserIdFromToken(token);
            if (userId == null)
            {
                return (false, null);
            }

            var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
            if (user == null || user.Status != UserStatus.Active)
            {
                return (false, null);
            }

            return (true, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "令牌验证失败");
            return (false, null);
        }
    }

    public async Task<(bool Success, string Message)> SendPasswordResetEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                // 为了安全，即使用户不存在也返回成功
                return (true, "如果邮箱存在，重置链接已发送");
            }

            // 生成重置令牌
            user.PasswordResetToken = GenerateToken();
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1); // 1小时过期

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            // TODO: 发送邮件（需要实现邮件服务）
            _logger.LogInformation("密码重置邮件已准备发送: {Email}", email);

            return (true, "重置链接已发送");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送密码重置邮件失败: {Email}", email);
            return (false, "发送失败");
        }
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByPasswordResetTokenAsync(token, cancellationToken);
            if (user == null)
            {
                return (false, "重置令牌无效或已过期");
            }

            // 更新密码
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("用户密码重置成功: {Email}", user.Email);
            return (true, "密码重置成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "密码重置失败");
            return (false, "重置失败");
        }
    }

    public async Task<(bool Success, string Message)> ConfirmEmailAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByEmailConfirmationTokenAsync(token, cancellationToken);
            if (user == null)
            {
                return (false, "验证令牌无效");
            }

            user.EmailConfirmed = true;
            user.EmailConfirmationToken = null;

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("用户邮箱验证成功: {Email}", user.Email);
            return (true, "邮箱验证成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "邮箱验证失败");
            return (false, "验证失败");
        }
    }

    public async Task<(bool Success, string? Token)> RefreshTokenAsync(string currentToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var (valid, user) = await ValidateTokenAsync(currentToken, cancellationToken);
            if (!valid || user == null)
            {
                return (false, null);
            }

            var newToken = _jwtService.GenerateToken(user);
            return (true, newToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "令牌刷新失败");
            return (false, null);
        }
    }

    public async Task<User?> GetUserByKeyAsync(string userKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var keyInfo = await _userKeyRepository.GetByKeyAsync(userKey, cancellationToken);
            return keyInfo?.User;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据密钥获取用户失败: {UserKey}", userKey);
            return null;
        }
    }

    private static string GenerateToken()
    {
        const int tokenLength = 32;
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        
        var token = new char[tokenLength];
        for (int i = 0; i < tokenLength; i++)
        {
            token[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
        }
        
        return new string(token);
    }
} 