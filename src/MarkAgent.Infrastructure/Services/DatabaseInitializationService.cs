using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MarkAgent.Domain.Entities;
using MarkAgent.Domain.Enums;
using MarkAgent.Domain.Repositories;
using MarkAgent.Domain.ValueObjects;
using BCrypt.Net;

namespace MarkAgent.Infrastructure.Services;

public class DatabaseInitializationService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserStatisticsRepository _userStatisticsRepository;
    private readonly ILogger<DatabaseInitializationService> _logger;
    private readonly AdminAccountOptions _adminOptions;

    public DatabaseInitializationService(
        IUserRepository userRepository,
        IUserStatisticsRepository userStatisticsRepository,
        ILogger<DatabaseInitializationService> logger,
        IOptions<AdminAccountOptions> adminOptions)
    {
        _userRepository = userRepository;
        _userStatisticsRepository = userStatisticsRepository;
        _logger = logger;
        _adminOptions = adminOptions.Value;
    }

    public async Task InitializeAsync()
    {
        await CreateDefaultAdminAsync();
    }

    private async Task CreateDefaultAdminAsync()
    {
        try
        {
            var adminEmail = Email.Create(_adminOptions.Email);
            
            // 检查管理员账号是否已存在
            var existingAdmin = await _userRepository.GetByEmailAsync(adminEmail);
            if (existingAdmin != null)
            {
                _logger.LogInformation("Default admin account already exists");
                return;
            }

            // 创建管理员账号
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(_adminOptions.Password);
            var adminUserKey = UserKey.Create(_adminOptions.UserKey);
            
            var adminUser = new User(adminEmail, passwordHash, adminUserKey, UserRole.Admin);
            adminUser.VerifyEmail(); // 管理员账号默认已验证邮箱
            
            await _userRepository.AddAsync(adminUser);

            // 创建管理员统计记录
            var adminStats = new UserStatistics(adminUser.Id);
            await _userStatisticsRepository.AddAsync(adminStats);

            _logger.LogInformation("Default admin account created successfully with email: {Email}", _adminOptions.Email);
            _logger.LogInformation("Admin user key: {UserKey}", adminUserKey.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create default admin account");
            throw;
        }
    }
}

public class AdminAccountOptions
{
    public const string SectionName = "AdminAccount";

    public string Email { get; set; } = "admin@markagent.com";
    public string Password { get; set; } = "Admin123!";
    public string UserKey { get; set; } = "sk-admin-default-key-12345";
}