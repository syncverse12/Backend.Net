using System;
using Graduation_Project.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project
{
    public class DatabaseDbContext : IdentityDbContext<User, Role, string>
    {
        public DatabaseDbContext(DbContextOptions<DatabaseDbContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(Program).Assembly);
        }
    }
}
