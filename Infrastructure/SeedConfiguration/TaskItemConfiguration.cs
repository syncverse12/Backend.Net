using SyncVerse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SyncVerse.Infrastructure.Persistence.Configurations
{
    public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
    {
        public void Configure(EntityTypeBuilder<TaskItem> builder)
        {
            builder.ToTable("Tasks");

            builder.Property(t => t.Title).IsRequired().HasMaxLength(250);
            builder.Property(t => t.Description).HasMaxLength(2000);

            builder.HasOne(t => t.CreatedByUser)
                .WithMany()
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.AssignedToUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.ReviewedByUser)
                .WithMany()
                .HasForeignKey(t => t.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}