using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncVerse.Domain.Entities;

namespace SyncVerse.Infrastructure.SeedConfiguration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.SeniorityLevel)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(u => u.Department)
                .IsRequired()
                .HasConversion<int>();

            builder.HasIndex(u => u.Department);
            builder.HasIndex(u => u.SeniorityLevel);
        }
    }
}