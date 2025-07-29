using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BCrypt.Net;

namespace MarkAgent.Host.Services;

/// <summary>
/// OAuth服务实现
/// </summary>
public class OAuthService : IOAuthService
{
    private readonly IOAuthConfigRepository _oauthConfigRepository;
    private readonly IUserOAuthBindingRepository _bindingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserKeyService _userKeyService;
    private readonly JwtService _jwtService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<OAuthOptions> _oauthOptions;
    private readonly ILogger<OAuthService> _logger;

    public OAuthService(
        IOAuthConfigRepository oauthConfigRepository,
        IUserOAuthBindingRepository bindingRepository,
        IUserRepository userRepository,
        IUserKeyService userKeyService,
        JwtService jwtService,
        IHttpClientFactory httpClientFactory,
        IOptions<OAuthOptions> oauthOptions,
        ILogger<OAuthService> logger)
    {
        _oauthConfigRepository = oauthConfigRepository;
        _bindingRepository = bindingRepository;
        _userRepository = userRepository;
        _userKeyService = userKeyService;
        _jwtService = jwtService;
        _httpClientFactory = httpClientFactory;
        _oauthOptions = oauthOptions;
        _logger = logger;
    }

    public async Task<(bool Success, string? AuthorizeUrl, string? State, string Message)> GetAuthorizeUrlAsync(string provider, string? redirectUri = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var config = await GetOAuthConfigAsync(provider, cancellationToken);
            if (config == null || !config.IsEnabled)
            {
                return (false, null, null, "不支持的OAuth提供商");
            }

            var state = GenerateState();
            var actualRedirectUri = redirectUri ?? config.RedirectUri;
            
            var authorizeUrl = BuildAuthorizeUrl(config, actualRedirectUri, state);
            
            return (true, authorizeUrl, state, "成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取OAuth授权URL失败: {Provider}", provider);
            return (false, null, null, "获取授权URL失败");
        }
    }

    public async Task<(bool Success, string? Token, User? User, bool IsNewUser, string Message)> HandleCallbackAsync(string provider, string code, string state, string? redirectUri = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var config = await GetOAuthConfigAsync(provider, cancellationToken);
            if (config == null || !config.IsEnabled)
            {
                return (false, null, null, false, "不支持的OAuth提供商");
            }

            // 获取访问令牌
            var tokenResult = await ExchangeCodeForTokenAsync(config, code, redirectUri ?? config.RedirectUri, cancellationToken);
            if (!tokenResult.Success)
            {
                return (false, null, null, false, tokenResult.Message);
            }

            // 获取用户信息
            var userInfoResult = await GetUserInfoAsync(config, tokenResult.AccessToken!, cancellationToken);
            if (!userInfoResult.Success)
            {
                return (false, null, null, false, userInfoResult.Message);
            }

            var oauthUserInfo = userInfoResult.UserInfo!;
            oauthUserInfo = oauthUserInfo with 
            { 
                AccessToken = tokenResult.AccessToken,
                RefreshToken = tokenResult.RefreshToken,
                TokenExpiredAt = tokenResult.ExpiresAt
            };

            // 检查是否已有绑定
            var existingBinding = await _bindingRepository.GetByProviderAndProviderUserIdAsync(provider, oauthUserInfo.ProviderUserId, cancellationToken);
            if (existingBinding != null)
            {
                // 更新令牌信息
                await _bindingRepository.UpdateTokenAsync(existingBinding.Id, 
                    oauthUserInfo.AccessToken, 
                    oauthUserInfo.RefreshToken, 
                    oauthUserInfo.TokenExpiredAt, 
                    cancellationToken);
                await _bindingRepository.SaveChangesAsync(cancellationToken);

                var token = _jwtService.GenerateToken(existingBinding.User);
                return (true, token, existingBinding.User, false, "登录成功");
            }

            // 创建新用户或获取现有用户
            var userResult = await GetOrCreateUserFromOAuthAsync(provider, oauthUserInfo, cancellationToken);
            if (!userResult.Success)
            {
                return (false, null, null, false, userResult.Message);
            }

            var jwtToken = _jwtService.GenerateToken(userResult.User!);
            return (true, jwtToken, userResult.User, userResult.IsNewUser, "登录成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OAuth回调处理失败: {Provider}", provider);
            return (false, null, null, false, "OAuth登录失败");
        }
    }

    public async Task<(bool Success, string Message)> BindOAuthAccountAsync(Guid userId, string provider, string code, string state, string? redirectUri = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return (false, "用户不存在");
            }

            // 检查是否已绑定该提供商
            var existingBinding = await _bindingRepository.GetByUserIdAndProviderAsync(userId, provider, cancellationToken);
            if (existingBinding != null)
            {
                return (false, "已绑定该OAuth账号");
            }

            var config = await GetOAuthConfigAsync(provider, cancellationToken);
            if (config == null || !config.IsEnabled)
            {
                return (false, "不支持的OAuth提供商");
            }

            // 获取访问令牌
            var tokenResult = await ExchangeCodeForTokenAsync(config, code, redirectUri ?? config.RedirectUri, cancellationToken);
            if (!tokenResult.Success)
            {
                return (false, tokenResult.Message);
            }

            // 获取用户信息
            var userInfoResult = await GetUserInfoAsync(config, tokenResult.AccessToken!, cancellationToken);
            if (!userInfoResult.Success)
            {
                return (false, userInfoResult.Message);
            }

            var oauthUserInfo = userInfoResult.UserInfo!;

            // 检查该OAuth账号是否已被其他用户绑定
            var providerBinding = await _bindingRepository.GetByProviderAndProviderUserIdAsync(provider, oauthUserInfo.ProviderUserId, cancellationToken);
            if (providerBinding != null)
            {
                return (false, "该OAuth账号已被其他用户绑定");
            }

            // 创建绑定
            var binding = new UserOAuthBinding
            {
                UserId = userId,
                Provider = provider,
                ProviderUserId = oauthUserInfo.ProviderUserId,
                ProviderUserName = oauthUserInfo.UserName,
                ProviderEmail = oauthUserInfo.Email,
                ProviderAvatarUrl = oauthUserInfo.AvatarUrl,
                AccessToken = tokenResult.AccessToken,
                RefreshToken = tokenResult.RefreshToken,
                TokenExpiredAt = tokenResult.ExpiresAt,
                ExtraInfo = oauthUserInfo.ExtraInfo != null ? JsonSerializer.Serialize(oauthUserInfo.ExtraInfo) : null
            };

            await _bindingRepository.AddAsync(binding, cancellationToken);
            await _bindingRepository.SaveChangesAsync(cancellationToken);

            return (true, "OAuth账号绑定成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OAuth账号绑定失败: {Provider}, {UserId}", provider, userId);
            return (false, "绑定失败");
        }
    }

    public async Task<(bool Success, string Message)> UnbindOAuthAccountAsync(Guid userId, string provider, CancellationToken cancellationToken = default)
    {
        try
        {
            var binding = await _bindingRepository.GetByUserIdAndProviderAsync(userId, provider, cancellationToken);
            if (binding == null)
            {
                return (false, "未找到该OAuth绑定");
            }

            await _bindingRepository.DeleteAsync(binding, cancellationToken);
            await _bindingRepository.SaveChangesAsync(cancellationToken);

            return (true, "OAuth账号解绑成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OAuth账号解绑失败: {Provider}, {UserId}", provider, userId);
            return (false, "解绑失败");
        }
    }

    public async Task<IEnumerable<UserOAuthBinding>> GetUserOAuthBindingsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _bindingRepository.GetByUserIdAsync(userId, cancellationToken);
    }

    public async Task<(bool Success, string Message)> RefreshTokenAsync(Guid bindingId, CancellationToken cancellationToken = default)
    {
        try
        {
            var binding = await _bindingRepository.GetByIdAsync(bindingId, cancellationToken);
            if (binding == null)
            {
                return (false, "绑定不存在");
            }

            if (string.IsNullOrEmpty(binding.RefreshToken))
            {
                return (false, "无刷新令牌");
            }

            var config = await GetOAuthConfigAsync(binding.Provider, cancellationToken);
            if (config == null)
            {
                return (false, "OAuth配置不存在");
            }

            var tokenResult = await RefreshAccessTokenAsync(config, binding.RefreshToken, cancellationToken);
            if (!tokenResult.Success)
            {
                return (false, tokenResult.Message);
            }

            await _bindingRepository.UpdateTokenAsync(bindingId, 
                tokenResult.AccessToken, 
                tokenResult.RefreshToken ?? binding.RefreshToken, 
                tokenResult.ExpiresAt, 
                cancellationToken);
            await _bindingRepository.SaveChangesAsync(cancellationToken);

            return (true, "令牌刷新成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刷新OAuth令牌失败: {BindingId}", bindingId);
            return (false, "刷新令牌失败");
        }
    }

    public async Task<IEnumerable<OAuthConfig>> GetEnabledProvidersAsync(CancellationToken cancellationToken = default)
    {
        var enabledConfigs = new List<OAuthConfig>();
        
        // 首先添加配置文件中启用的提供商
        foreach (var providerConfig in _oauthOptions.Value.Providers)
        {
            if (providerConfig.Value.IsEnabled && !string.IsNullOrEmpty(providerConfig.Value.ClientId))
            {
                enabledConfigs.Add(new OAuthConfig
                {
                    Provider = providerConfig.Key,
                    ClientId = providerConfig.Value.ClientId,
                    ClientSecret = providerConfig.Value.ClientSecret,
                    AuthorizeUrl = providerConfig.Value.AuthorizeUrl,
                    TokenUrl = providerConfig.Value.TokenUrl,
                    UserInfoUrl = providerConfig.Value.UserInfoUrl,
                    RedirectUri = providerConfig.Value.RedirectUri,
                    Scope = providerConfig.Value.Scope,
                    IsEnabled = providerConfig.Value.IsEnabled,
                    Sort = providerConfig.Value.Sort,
                    Description = providerConfig.Value.Description
                });
            }
        }
        
        // 然后添加数据库中启用但配置文件中不存在或未启用的提供商
        var dbConfigs = await _oauthConfigRepository.GetEnabledConfigsAsync(cancellationToken);
        foreach (var dbConfig in dbConfigs)
        {
            if (!enabledConfigs.Any(c => c.Provider == dbConfig.Provider))
            {
                enabledConfigs.Add(dbConfig);
            }
        }
        
        return enabledConfigs.OrderBy(c => c.Sort).ThenBy(c => c.Provider);
    }

    public async Task<(bool Success, User? User, bool IsNewUser, string Message)> GetOrCreateUserFromOAuthAsync(string provider, OAuthUserInfo userInfo, CancellationToken cancellationToken = default)
    {
        try
        {
            User? user = null;
            bool isNewUser = false;

            // 如果有邮箱，尝试查找现有用户
            if (!string.IsNullOrEmpty(userInfo.Email))
            {
                user = await _userRepository.GetByEmailAsync(userInfo.Email, cancellationToken);
            }

            // 如果没找到用户，创建新用户
            if (user == null)
            {
                var userName = userInfo.UserName ?? $"{provider}_user_{GenerateRandomString(6)}";
                var email = userInfo.Email ?? $"{provider}_user_{GenerateRandomString(6)}@{provider}.oauth";

                // 确保用户名唯一
                var originalUserName = userName;
                int counter = 1;
                while (await _userRepository.IsUserNameExistsAsync(userName, cancellationToken))
                {
                    userName = $"{originalUserName}_{counter}";
                    counter++;
                }

                // 确保邮箱唯一
                var originalEmail = email;
                counter = 1;
                while (await _userRepository.IsEmailExistsAsync(email, cancellationToken))
                {
                    var emailParts = originalEmail.Split('@');
                    email = $"{emailParts[0]}_{counter}@{emailParts[1]}";
                    counter++;
                }

                user = new User
                {
                    Email = email,
                    UserName = userName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(GenerateRandomString(32)), // 随机密码
                    EmailConfirmed = !string.IsNullOrEmpty(userInfo.Email), // 如果有真实邮箱则视为已验证
                    Status = UserStatus.Active
                };

                await _userRepository.AddAsync(user, cancellationToken);
                await _userRepository.SaveChangesAsync(cancellationToken);

                // 为新用户创建默认密钥
                var keyResult = await _userKeyService.CreateKeyAsync(user.Id, "默认密钥", "OAuth登录自动创建的默认密钥", cancellationToken);
                if (keyResult.Success && keyResult.UserKey != null)
                {
                    await _userKeyService.SetDefaultKeyAsync(user.Id, keyResult.UserKey.Id, cancellationToken);
                }

                isNewUser = true;
                _logger.LogInformation("通过OAuth创建新用户: {Provider}, {UserId}", provider, user.Id);
            }

            // 创建或更新OAuth绑定
            var binding = await _bindingRepository.GetByUserIdAndProviderAsync(user.Id, provider, cancellationToken);
            if (binding == null)
            {
                binding = new UserOAuthBinding
                {
                    UserId = user.Id,
                    Provider = provider,
                    ProviderUserId = userInfo.ProviderUserId,
                    ProviderUserName = userInfo.UserName,
                    ProviderEmail = userInfo.Email,
                    ProviderAvatarUrl = userInfo.AvatarUrl,
                    AccessToken = userInfo.AccessToken,
                    RefreshToken = userInfo.RefreshToken,
                    TokenExpiredAt = userInfo.TokenExpiredAt,
                    ExtraInfo = userInfo.ExtraInfo != null ? JsonSerializer.Serialize(userInfo.ExtraInfo) : null
                };

                await _bindingRepository.AddAsync(binding, cancellationToken);
            }
            else
            {
                // 更新现有绑定
                binding.ProviderUserName = userInfo.UserName;
                binding.ProviderEmail = userInfo.Email;
                binding.ProviderAvatarUrl = userInfo.AvatarUrl;
                binding.AccessToken = userInfo.AccessToken;
                binding.RefreshToken = userInfo.RefreshToken;
                binding.TokenExpiredAt = userInfo.TokenExpiredAt;
                binding.LastSyncAt = DateTime.UtcNow;
                binding.ExtraInfo = userInfo.ExtraInfo != null ? JsonSerializer.Serialize(userInfo.ExtraInfo) : null;

                await _bindingRepository.UpdateAsync(binding, cancellationToken);
            }

            await _bindingRepository.SaveChangesAsync(cancellationToken);

            return (true, user, isNewUser, "成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "通过OAuth获取或创建用户失败: {Provider}", provider);
            return (false, null, false, "操作失败");
        }
    }

    #region 私有方法

    private async Task<OAuthConfig?> GetOAuthConfigAsync(string provider, CancellationToken cancellationToken = default)
    {
        // 首先尝试从配置文件获取
        if (_oauthOptions.Value.Providers.TryGetValue(provider, out var configOption))
        {
            // 如果配置文件中有配置且启用，使用配置文件的配置
            if (configOption.IsEnabled && !string.IsNullOrEmpty(configOption.ClientId))
            {
                return new OAuthConfig
                {
                    Provider = provider,
                    ClientId = configOption.ClientId,
                    ClientSecret = configOption.ClientSecret,
                    AuthorizeUrl = configOption.AuthorizeUrl,
                    TokenUrl = configOption.TokenUrl,
                    UserInfoUrl = configOption.UserInfoUrl,
                    RedirectUri = configOption.RedirectUri,
                    Scope = configOption.Scope,
                    IsEnabled = configOption.IsEnabled,
                    Sort = configOption.Sort,
                    Description = configOption.Description
                };
            }
        }

        // 如果配置文件中没有或未启用，尝试从数据库获取
        var dbConfig = await _oauthConfigRepository.GetByProviderAsync(provider, cancellationToken);
        return dbConfig;
    }

    private string GenerateState()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    private string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
        }
        return new string(result);
    }

    private string BuildAuthorizeUrl(OAuthConfig config, string redirectUri, string state)
    {
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = config.ClientId,
            ["redirect_uri"] = redirectUri,
            ["state"] = state,
            ["response_type"] = "code"
        };

        if (!string.IsNullOrEmpty(config.Scope))
        {
            parameters["scope"] = config.Scope;
        }

        var queryString = string.Join("&", parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
        return $"{config.AuthorizeUrl}?{queryString}";
    }

    private async Task<(bool Success, string? AccessToken, string? RefreshToken, DateTime? ExpiresAt, string Message)> ExchangeCodeForTokenAsync(OAuthConfig config, string code, string redirectUri, CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            
            var parameters = new Dictionary<string, string>
            {
                ["client_id"] = config.ClientId,
                ["client_secret"] = config.ClientSecret,
                ["code"] = code,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code"
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await httpClient.PostAsync(config.TokenUrl, content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("获取访问令牌失败: {StatusCode}, {Content}", response.StatusCode, errorContent);
                return (false, null, null, null, "获取访问令牌失败");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var accessToken = tokenResponse.TryGetProperty("access_token", out var at) ? at.GetString() : null;
            var refreshToken = tokenResponse.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
            
            DateTime? expiresAt = null;
            if (tokenResponse.TryGetProperty("expires_in", out var expiresIn) && expiresIn.TryGetInt32(out var seconds))
            {
                expiresAt = DateTime.UtcNow.AddSeconds(seconds);
            }

            if (string.IsNullOrEmpty(accessToken))
            {
                return (false, null, null, null, "访问令牌为空");
            }

            return (true, accessToken, refreshToken, expiresAt, "成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "交换代码获取令牌失败");
            return (false, null, null, null, "交换令牌失败");
        }
    }

    private async Task<(bool Success, OAuthUserInfo? UserInfo, string Message)> GetUserInfoAsync(OAuthConfig config, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.GetAsync(config.UserInfoUrl, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("获取用户信息失败: {StatusCode}, {Content}", response.StatusCode, errorContent);
                return (false, null, "获取用户信息失败");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var userResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var userInfo = ParseUserInfo(config.Provider, userResponse);
            return (true, userInfo, "成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户信息失败");
            return (false, null, "获取用户信息失败");
        }
    }

    private async Task<(bool Success, string? AccessToken, string? RefreshToken, DateTime? ExpiresAt, string Message)> RefreshAccessTokenAsync(OAuthConfig config, string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            
            var parameters = new Dictionary<string, string>
            {
                ["client_id"] = config.ClientId,
                ["client_secret"] = config.ClientSecret,
                ["refresh_token"] = refreshToken,
                ["grant_type"] = "refresh_token"
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await httpClient.PostAsync(config.TokenUrl, content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("刷新访问令牌失败: {StatusCode}, {Content}", response.StatusCode, errorContent);
                return (false, null, null, null, "刷新访问令牌失败");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var accessToken = tokenResponse.TryGetProperty("access_token", out var at) ? at.GetString() : null;
            var newRefreshToken = tokenResponse.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
            
            DateTime? expiresAt = null;
            if (tokenResponse.TryGetProperty("expires_in", out var expiresIn) && expiresIn.TryGetInt32(out var seconds))
            {
                expiresAt = DateTime.UtcNow.AddSeconds(seconds);
            }

            if (string.IsNullOrEmpty(accessToken))
            {
                return (false, null, null, null, "访问令牌为空");
            }

            return (true, accessToken, newRefreshToken, expiresAt, "成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刷新访问令牌失败");
            return (false, null, null, null, "刷新令牌失败");
        }
    }

    private OAuthUserInfo ParseUserInfo(string provider, JsonElement userResponse)
    {
        return provider.ToLower() switch
        {
            OAuthProviders.GitHub => ParseGitHubUserInfo(userResponse),
            OAuthProviders.Gitee => ParseGiteeUserInfo(userResponse),
            OAuthProviders.Google => ParseGoogleUserInfo(userResponse),
            _ => ParseGenericUserInfo(userResponse)
        };
    }

    private OAuthUserInfo ParseGitHubUserInfo(JsonElement userResponse)
    {
        return new OAuthUserInfo
        {
            ProviderUserId = userResponse.TryGetProperty("id", out var id) ? id.ToString() : "",
            UserName = userResponse.TryGetProperty("login", out var login) ? login.GetString() : null,
            Email = userResponse.TryGetProperty("email", out var email) ? email.GetString() : null,
            AvatarUrl = userResponse.TryGetProperty("avatar_url", out var avatar) ? avatar.GetString() : null,
            ExtraInfo = new Dictionary<string, object>
            {
                ["name"] = userResponse.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "",
                ["bio"] = userResponse.TryGetProperty("bio", out var bio) ? bio.GetString() ?? "" : "",
                ["company"] = userResponse.TryGetProperty("company", out var company) ? company.GetString() ?? "" : "",
                ["location"] = userResponse.TryGetProperty("location", out var location) ? location.GetString() ?? "" : "",
                ["blog"] = userResponse.TryGetProperty("blog", out var blog) ? blog.GetString() ?? "" : ""
            }
        };
    }

    private OAuthUserInfo ParseGiteeUserInfo(JsonElement userResponse)
    {
        return new OAuthUserInfo
        {
            ProviderUserId = userResponse.TryGetProperty("id", out var id) ? id.ToString() : "",
            UserName = userResponse.TryGetProperty("login", out var login) ? login.GetString() : null,
            Email = userResponse.TryGetProperty("email", out var email) ? email.GetString() : null,
            AvatarUrl = userResponse.TryGetProperty("avatar_url", out var avatar) ? avatar.GetString() : null,
            ExtraInfo = new Dictionary<string, object>
            {
                ["name"] = userResponse.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "",
                ["bio"] = userResponse.TryGetProperty("bio", out var bio) ? bio.GetString() ?? "" : "",
                ["company"] = userResponse.TryGetProperty("company", out var company) ? company.GetString() ?? "" : "",
                ["profession"] = userResponse.TryGetProperty("profession", out var profession) ? profession.GetString() ?? "" : ""
            }
        };
    }

    private OAuthUserInfo ParseGoogleUserInfo(JsonElement userResponse)
    {
        return new OAuthUserInfo
        {
            ProviderUserId = userResponse.TryGetProperty("sub", out var sub) ? sub.GetString() ?? "" : "",
            UserName = userResponse.TryGetProperty("name", out var name) ? name.GetString() : null,
            Email = userResponse.TryGetProperty("email", out var email) ? email.GetString() : null,
            AvatarUrl = userResponse.TryGetProperty("picture", out var picture) ? picture.GetString() : null,
            ExtraInfo = new Dictionary<string, object>
            {
                ["given_name"] = userResponse.TryGetProperty("given_name", out var givenName) ? givenName.GetString() ?? "" : "",
                ["family_name"] = userResponse.TryGetProperty("family_name", out var familyName) ? familyName.GetString() ?? "" : "",
                ["locale"] = userResponse.TryGetProperty("locale", out var locale) ? locale.GetString() ?? "" : "",
                ["email_verified"] = userResponse.TryGetProperty("email_verified", out var emailVerified) ? emailVerified.GetBoolean() : false
            }
        };
    }

    private OAuthUserInfo ParseGenericUserInfo(JsonElement userResponse)
    {
        return new OAuthUserInfo
        {
            ProviderUserId = userResponse.TryGetProperty("id", out var id) ? id.ToString() : "",
            UserName = userResponse.TryGetProperty("name", out var name) ? name.GetString() : 
                       userResponse.TryGetProperty("username", out var username) ? username.GetString() : null,
            Email = userResponse.TryGetProperty("email", out var email) ? email.GetString() : null,
            AvatarUrl = userResponse.TryGetProperty("avatar", out var avatar) ? avatar.GetString() : 
                        userResponse.TryGetProperty("avatar_url", out var avatarUrl) ? avatarUrl.GetString() : null
        };
    }

    #endregion
} 