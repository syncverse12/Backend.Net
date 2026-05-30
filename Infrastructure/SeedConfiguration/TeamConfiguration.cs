using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncVerse.Domain.Entities;

namespace SyncVerse.Infrastructure.SeedConfiguration
{
    public class TeamConfiguration : IEntityTypeConfiguration<Team>
    {
        public void Configure(EntityTypeBuilder<Team> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Description)
                .HasMaxLength(500);

            builder.Property(t => t.Specialization)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(t => t.Department)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(t => t.WorkspaceId)
                .IsRequired();

            builder.HasOne(t => t.Workspace)
                .WithMany()
                .HasForeignKey(t => t.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.CreatedByManager)
                .WithMany()
                .HasForeignKey(t => t.CreatedByManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.TeamMembers)
                .WithOne(tm => tm.Team)
                .HasForeignKey(tm => tm.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(t => t.Name);
            builder.HasIndex(t => t.Department);
            builder.HasIndex(t => t.CreatedByManagerId);
            builder.HasIndex(t => t.WorkspaceId);
        }
    }
}
