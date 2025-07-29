using Microsoft.EntityFrameworkCore;
using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Infrastructure.Data;

/// <summary>
/// 统计数据数据库上下文
/// </summary>
public class StatisticsDbContext : DbContext
{
    public StatisticsDbContext(DbContextOptions<StatisticsDbContext> options) : base(options)
    {
    }

    public DbSet<ToolUsageRecord> ToolUsageRecords { get; set; }
    public DbSet<DailyToolStatistics> DailyToolStatistics { get; set; }
    public DbSet<ClientConnectionRecord> ClientConnectionRecords { get; set; }
    public DbSet<DailyClientStatistics> DailyClientStatistics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ToolUsageRecord 配置
        modelBuilder.Entity<ToolUsageRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ToolName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.ParametersJson).HasColumnType("TEXT");
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);

            // 索引优化查询性能
            entity.HasIndex(e => e.ToolName);
            entity.HasIndex(e => e.StartTime);
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => new { e.ToolName, e.StartTime });
            entity.HasIndex(e => new { e.IsSuccess, e.StartTime });
        });

        // DailyToolStatistics 配置
        modelBuilder.Entity<DailyToolStatistics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ToolName).IsRequired().HasMaxLength(100);

            // 复合唯一索引：确保每天每个工具只有一条统计记录
            entity.HasIndex(e => new { e.Date, e.ToolName }).IsUnique();
            
            // 其他索引
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => e.ToolName);
        });

        // ClientConnectionRecord 配置
        modelBuilder.Entity<ClientConnectionRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClientName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ClientVersion).HasMaxLength(50);
            entity.Property(e => e.ClientTitle).HasMaxLength(200);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.ProtocolVersion).HasMaxLength(20);
            entity.Property(e => e.ClientCapabilities).HasColumnType("TEXT");
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);

            // 索引优化查询性能
            entity.HasIndex(e => e.ClientName);
            entity.HasIndex(e => e.SessionId).IsUnique();
            entity.HasIndex(e => e.ConnectionTime);
            entity.HasIndex(e => new { e.ClientName, e.ConnectionTime });
            entity.HasIndex(e => new { e.Status, e.ConnectionTime });
        });

        // DailyClientStatistics 配置
        modelBuilder.Entity<DailyClientStatistics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClientName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ClientVersion).HasMaxLength(50);

            // 复合唯一索引：确保每天每个客户端只有一条统计记录
            entity.HasIndex(e => new { e.Date, e.ClientName, e.ClientVersion }).IsUnique();
            
            // 其他索引
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => e.ClientName);
        });
    }
}