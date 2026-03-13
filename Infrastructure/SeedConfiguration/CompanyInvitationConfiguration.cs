using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncVerse.Domain.Entities;

namespace SyncVerse.Infrastructure.SeedConfiguration
{
    public class CompanyInvitationConfiguration : IEntityTypeConfiguration<CompanyInvitation>
    {
        public void Configure(EntityTypeBuilder<CompanyInvitation> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(i => i.InvitationToken)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(i => i.SeniorityLevel)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(i => i.Role)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(i => i.Status)
                .IsRequired()
                .HasConversion<int>();

            // ✅ Relationship with Team
            builder.HasOne(i => i.Team)
                .WithMany()
                .HasForeignKey(i => i.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.SentByHR)
                .WithMany()
                .HasForeignKey(i => i.SentByHRId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(i => i.InvitationToken).IsUnique();
            builder.HasIndex(i => i.Email);
        }
    }
}