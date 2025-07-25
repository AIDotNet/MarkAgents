using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MarkAgent.Domain.Entities;

namespace MarkAgent.Infrastructure.Data.Configurations;

public class ConversationSessionConfiguration : IEntityTypeConfiguration<ConversationSession>
{
    public void Configure(EntityTypeBuilder<ConversationSession> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.SessionName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.IsActive)
            .IsRequired();

        builder.Property(s => s.EndedAt);

        builder.Property(s => s.UserId)
            .IsRequired();

        // Indexes
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.IsActive);
        builder.HasIndex(s => new { s.UserId, s.IsActive });

        // Relationships
        builder.HasOne(s => s.User)
            .WithMany(u => u.ConversationSessions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.TodoItems)
            .WithOne(t => t.ConversationSession)
            .HasForeignKey(t => t.ConversationSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}