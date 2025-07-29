using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Infrastructure.Data;

/// <summary>
/// MarkAgent数据库上下文
/// </summary>
public class MarkAgentDbContext : DbContext
{
    public MarkAgentDbContext(DbContextOptions<MarkAgentDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// 用户表
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// 用户密钥表
    /// </summary>
    public DbSet<UserKey> UserKeys { get; set; }

    /// <summary>
    /// 对话表
    /// </summary>
    public DbSet<Conversation> Conversations { get; set; }

    /// <summary>
    /// Todo表
    /// </summary>
    public DbSet<Todo> Todos { get; set; }

    /// <summary>
    /// 用户统计表
    /// </summary>
    public DbSet<UserStatistics> UserStatistics { get; set; }

    /// <summary>
    /// 系统统计表
    /// </summary>
    public DbSet<SystemStatistics> SystemStatistics { get; set; }

    /// <summary>
    /// MCP工具配置表
    /// </summary>
    public DbSet<McpToolConfig> McpToolConfigs { get; set; }

    /// <summary>
    /// MCP工具使用记录表
    /// </summary>
    public DbSet<McpToolUsage> McpToolUsages { get; set; }

    /// <summary>
    /// 系统MCP工具表
    /// </summary>
    public DbSet<SystemMcpTool> SystemMcpTools { get; set; }

    /// <summary>
    /// 邮箱验证码表
    /// </summary>
    public DbSet<EmailVerificationCode> EmailVerificationCodes { get; set; }

    /// <summary>
    /// OAuth配置表
    /// </summary>
    public DbSet<OAuthConfig> OAuthConfigs { get; set; }

    /// <summary>
    /// 用户OAuth绑定表
    /// </summary>
    public DbSet<UserOAuthBinding> UserOAuthBindings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置软删除全局查询过滤器
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserKey>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Conversation>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Todo>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserStatistics>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SystemStatistics>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<McpToolConfig>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<McpToolUsage>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SystemMcpTool>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<EmailVerificationCode>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<OAuthConfig>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserOAuthBinding>().HasQueryFilter(e => !e.IsDeleted);

        // 配置User实体
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.UserName).IsUnique();
            entity.HasIndex(e => e.PasswordResetToken);
            entity.HasIndex(e => e.EmailConfirmationToken);
        });

        // 配置UserKey实体
        modelBuilder.Entity<UserKey>(entity =>
        { 
            entity.HasIndex(e => e.Key).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.Name }).IsUnique();

            entity.Property(x => x.McpToolsConfig)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions.Web),
                    v => string.IsNullOrEmpty(v)
                        ? new List<McpToolsConfig>()
                        : JsonSerializer.Deserialize<List<McpToolsConfig>>(v, JsonSerializerOptions.Web) ??
                          new List<McpToolsConfig>());

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserKeys)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 配置Conversation实体
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.UserKeyId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartedAt);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Conversations)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UserKey)
                .WithMany(uk => uk.Conversations)
                .HasForeignKey(e => e.UserKeyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 配置Todo实体
        modelBuilder.Entity<Todo>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.UserKeyId);
            entity.HasIndex(e => e.ConversationId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Priority);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UserKey)
                .WithMany()
                .HasForeignKey(e => e.UserKeyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Conversation)
                .WithMany(c => c.Todos)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 配置UserStatistics实体
        modelBuilder.Entity<UserStatistics>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.StatDate }).IsUnique();
            entity.HasIndex(e => new { e.UserKeyId, e.StatDate }).IsUnique();
            entity.HasIndex(e => e.StatDate);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UserKey)
                .WithMany()
                .HasForeignKey(e => e.UserKeyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 配置SystemStatistics实体
        modelBuilder.Entity<SystemStatistics>(entity => { entity.HasIndex(e => e.StatDate).IsUnique(); });

        // 配置McpToolConfig实体
        modelBuilder.Entity<McpToolConfig>(entity =>
        {
            entity.HasIndex(e => new { e.UserKeyId, e.ToolName }).IsUnique();
            entity.HasIndex(e => e.ToolName);
            entity.HasIndex(e => e.IsEnabled);

            entity.HasOne(e => e.UserKey)
                .WithMany(uk => uk.McpToolConfigs)
                .HasForeignKey(e => e.UserKeyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 配置McpToolUsage实体
        modelBuilder.Entity<McpToolUsage>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.UserKeyId);
            entity.HasIndex(e => e.ConversationId);
            entity.HasIndex(e => e.ToolName);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UserKey)
                .WithMany()
                .HasForeignKey(e => e.UserKeyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Conversation)
                .WithMany()
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 配置EmailVerificationCode实体
        modelBuilder.Entity<EmailVerificationCode>(entity =>
        {
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Code);
            entity.HasIndex(e => new { e.Email, e.Type });
            entity.HasIndex(e => new { e.Code, e.Type });
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => e.IsUsed);
        });

        // 配置OAuthConfig实体
        modelBuilder.Entity<OAuthConfig>(entity =>
        {
            entity.HasIndex(e => e.Provider).IsUnique();
            entity.HasIndex(e => e.IsEnabled);
            entity.HasIndex(e => e.Sort);
        });

        // 配置UserOAuthBinding实体
        modelBuilder.Entity<UserOAuthBinding>(entity =>
        {
            entity.HasIndex(e => new { e.Provider, e.ProviderUserId }).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.Provider }).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.BindTime);
            entity.HasIndex(e => e.TokenExpiredAt);

            entity.HasOne(e => e.User)
                .WithMany(u => u.OAuthBindings)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 配置SystemMcpTool实体
        modelBuilder.Entity<SystemMcpTool>(entity =>
        {
            entity.HasIndex(e => e.ToolName).IsUnique();
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsEnabled);
            entity.HasIndex(e => e.IsBuiltIn);
            entity.HasIndex(e => e.SortOrder);
            entity.HasIndex(e => e.TotalUsageCount);

            entity.Property(x => x.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions.Web),
                    v => string.IsNullOrEmpty(v)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(v, JsonSerializerOptions.Web) ??
                          new List<string>());
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 自动更新时间戳
        var entries = ChangeTracker.Entries<ICreationAudited>();

        foreach (var entry in ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity is ICreationAudited creationAudited)
                    {
                        creationAudited.CreatedAt = DateTime.UtcNow;
                    }

                    if (entry.Entity is IModificationAudited modificationAudited)
                    {
                        modificationAudited.UpdatedAt = DateTime.UtcNow;
                    }

                    break;
                case EntityState.Modified:
                    if (entry.Entity is IModificationAudited audited)
                    {
                        audited.UpdatedAt = DateTime.UtcNow;
                    }

                    break;
                case EntityState.Deleted:
                    // 实现软删除
                    entry.State = EntityState.Modified;
                    if (entry.Entity is ISoftDelete softDelete)
                    {
                        softDelete.IsDeleted = true;
                        softDelete.DeletedAt = DateTime.UtcNow;
                    }

                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}