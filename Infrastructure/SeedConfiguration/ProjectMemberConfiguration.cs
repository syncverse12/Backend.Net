using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncVerse.Domain.Entities;

namespace SyncVerse.Infrastructure.SeedConfiguration
{
    public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
    {
        public void Configure(EntityTypeBuilder<ProjectMember> builder)
        {
            builder.HasKey(pm => pm.Id);

            builder.Property(pm => pm.Role)
                .IsRequired()
                .HasConversion<int>();

            builder.HasOne(pm => pm.Project)
                .WithMany()
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pm => pm.User)
                .WithMany(u => u.ProjectMembers)
                .HasForeignKey(pm => pm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(pm => new { pm.ProjectId, pm.UserId }).IsUnique();
        }
    }
}