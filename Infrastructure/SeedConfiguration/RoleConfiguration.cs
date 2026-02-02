using Graduation_Project.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Graduation_Project.Infrastructure.SeedConfiguration
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasData( 
                new Role
                {
                    Id = "3f4c2b89-8ad1-4dfe-b61e-e3cbdf9a9d5c",
                    Name = "Manager",
                    NormalizedName = "MANAGER",
                    Description = "The Manager Role For The User"
                },
                new Role
                {
                    Id = "c4a8f0c1-3be2-4e35-9b7f-2ef45a6cb912",
                    Name = "Employee",
                    NormalizedName = "EMPLOYEE",
                    Description = "The Employee Role For The User"
                },
                new Role
                {
                    Id = "8e91d7bb-5c44-4c0a-9cd1-2730d1baf6a4",
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    Description = "The Admin Role For The User"
                }
            );
        }
    }
}
