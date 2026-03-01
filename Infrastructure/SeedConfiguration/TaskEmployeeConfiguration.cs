using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Synverse.Domain.Entities;

namespace SyncVerse.Infrastructure.SeedConfiguration
{
    public class TaskEmployeeConfiguration : IEntityTypeConfiguration<TaskEmployee>
    {
        public void Configure(EntityTypeBuilder<TaskEmployee> builder)
        {
            builder.ToTable("TaskEmployees");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TaskTitle)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.ProgressPercentage)
                   .HasDefaultValue(0);

            builder.Property(x => x.Status)
                   .HasConversion<int>();

            builder.Property(x => x.Priority)
                   .HasConversion<int>();

            builder.HasOne(x => x.AssignedUser)
                   .WithMany()
                   .HasForeignKey(x => x.AssignedUserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Project)
                   .WithMany(p => p.Tasks) 
                   .HasForeignKey(x => x.ProjectId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
