using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Synverse.Domain.Entities;

namespace Graduation_Project.Infrastructure.SeedConfiguration
{
    public class TimeLogConfiguration : IEntityTypeConfiguration<TimeLog>
    {
        public void Configure(EntityTypeBuilder<TimeLog> builder)
        {
            builder.ToTable("TimeLogs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DurationInMinutes)
                   .IsRequired();

            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
