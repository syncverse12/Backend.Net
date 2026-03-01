using SyncVerse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SyncVerse.Infrastructure.Persistence.Configurations
{
    public class TaskDependencyConfiguration : IEntityTypeConfiguration<TaskDependency>
    {
        public void Configure(EntityTypeBuilder<TaskDependency> builder)
        {
            builder.ToTable("TaskDependencies");
            builder.HasKey(td => new { td.TaskId, td.DependsOnTaskId });

            builder.HasOne(td => td.Task)
                .WithMany(t => t.Dependencies)
                .HasForeignKey(td => td.TaskId)
                .OnDelete(DeleteBehavior.Restrict); 

            builder.HasOne(td => td.DependsOnTask)
                .WithMany(t => t.DependentTasks)
                .HasForeignKey(td => td.DependsOnTaskId)
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}