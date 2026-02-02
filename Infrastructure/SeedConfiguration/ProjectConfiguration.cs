using Graduation_Project.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Graduation_Project.Infrastructure.Persistence.Configurations
{
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.ToTable("Projects");

            builder.Property(p => p.Budget)
                   .HasColumnType("decimal(18,2)"); 

            builder.Property(p => p.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(p => p.Description)
                   .IsRequired(false) 
                   .HasMaxLength(1000);

            builder.HasOne(p => p.Workspace)
                   .WithMany(w => w.Projects)
                   .HasForeignKey(p => p.WorkspaceId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.CreatedByUser)
                   .WithMany() 
                   .HasForeignKey(p => p.CreatedByUserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}