using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MarkAgent.Domain.Entities;
using MarkAgent.Domain.Enums;

namespace MarkAgent.Infrastructure.Data.Configurations;

public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.Priority)
            .IsRequired();

        builder.Property(t => t.DueDate);

        builder.Property(t => t.CompletedAt);

        builder.Property(t => t.UserId)
            .IsRequired();

        builder.Property(t => t.ConversationSessionId)
            .IsRequired();

        // Indexes
        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.ConversationSessionId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.DueDate);
        builder.HasIndex(t => new { t.UserId, t.Status });

        // Relationships
        builder.HasOne(t => t.User)
            .WithMany(u => u.TodoItems)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.ConversationSession)
            .WithMany(s => s.TodoItems)
            .HasForeignKey(t => t.ConversationSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}