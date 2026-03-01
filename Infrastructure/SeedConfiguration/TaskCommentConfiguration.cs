using SyncVerse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SyncVerse.Infrastructure.SeedConfiguration
{
    public class TaskCommentConfiguration : IEntityTypeConfiguration<TaskComment>
    {
        public void Configure(EntityTypeBuilder<TaskComment> builder)
        {
            builder.ToTable("TaskComments");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(2000);

            builder.HasOne(c => c.Task)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(c => c.TaskId);
            builder.HasIndex(c => c.UserId);
            builder.HasIndex(c => c.ParentCommentId);
        }
    }
}
