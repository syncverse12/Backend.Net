using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Graduation_Project.Domain.Entities;

namespace Graduation_Project.Infrastructure.SeedConfiguration
{
    public class TimeLogConfiguration : IEntityTypeConfiguration<TimeLog>
    {
        public void Configure(EntityTypeBuilder<TimeLog> builder)
        {
            builder.ToTable("TimeLogs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TaskId)
                   .IsRequired();

            builder.Property(x => x.UserId)
                   .IsRequired();

            builder.Property(x => x.DurationInMinutes)
                   .IsRequired();

            builder.Property(x => x.Notes)
                   .HasMaxLength(500);

            builder.HasOne(x => x.Task)
                   .WithMany(t => t.TimeLogs)
                   .HasForeignKey(x => x.TaskId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => x.TaskId);
            builder.HasIndex(x => x.UserId);
        }
    }

}
