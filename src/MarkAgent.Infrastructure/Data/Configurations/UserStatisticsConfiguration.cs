using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MarkAgent.Domain.Entities;

namespace MarkAgent.Infrastructure.Data.Configurations;

public class UserStatisticsConfiguration : IEntityTypeConfiguration<UserStatistics>
{
    public void Configure(EntityTypeBuilder<UserStatistics> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.TotalTodoCreated)
            .IsRequired();

        builder.Property(s => s.TotalTodoCompleted)
            .IsRequired();

        builder.Property(s => s.TotalConversationSessions)
            .IsRequired();

        builder.Property(s => s.LastActivityAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(s => s.UserId).IsUnique();
        builder.HasIndex(s => s.LastActivityAt);

        // Relationships
        builder.HasOne(s => s.User)
            .WithOne()
            .HasForeignKey<UserStatistics>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}