using Microsoft.EntityFrameworkCore;
using MarkAgent.Domain.Entities;
using MarkAgent.Domain.ValueObjects;
using MarkAgent.Infrastructure.Data.Configurations;

namespace MarkAgent.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<TodoItem> TodoItems { get; set; }
    public DbSet<ConversationSession> ConversationSessions { get; set; }
    public DbSet<UserStatistics> UserStatistics { get; set; }
    public DbSet<UserApiKey> UserApiKeys { get; set; }
    public DbSet<McpService> McpServices { get; set; }
    public DbSet<UserKeyMcpService> UserKeyMcpServices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new TodoItemConfiguration());
        modelBuilder.ApplyConfiguration(new ConversationSessionConfiguration());
        modelBuilder.ApplyConfiguration(new UserStatisticsConfiguration());
        modelBuilder.ApplyConfiguration(new UserApiKeyConfiguration());
        modelBuilder.ApplyConfiguration(new McpServiceConfiguration());
        modelBuilder.ApplyConfiguration(new UserKeyMcpServiceConfiguration());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Configure value object conversions
        configurationBuilder
            .Properties<Email>()
            .HaveConversion<EmailConverter>();

        configurationBuilder
            .Properties<UserKey>()
            .HaveConversion<UserKeyConverter>();
    }
}