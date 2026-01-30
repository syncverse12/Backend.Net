using Graduation_Project.Application.Interfaces.Task.Manager;
using Graduation_Project.Domain.Common;
using Graduation_Project.Domain.Entities;
using Graduation_Project.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Synverse.Domain.Entities;
using System.Linq.Expressions;

namespace Graduation_Project.Infrastructure.Data
{
    public class DatabaseDbContext : IdentityDbContext<User, Role, string>
    {
        private readonly ICurrentUserService _currentUserService;

        public DatabaseDbContext(
            DbContextOptions<DatabaseDbContext> options,
            ICurrentUserService currentUserService)
            : base(options)
        {
            _currentUserService = currentUserService;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInfo();
            return await base.SaveChangesAsync(cancellationToken);
        }
        public DbSet<TaskEmployee> TaskEmployees => Set<TaskEmployee>();
        public DbSet<TimeLog> TimeLogs => Set<TimeLog>();
        public DbSet<Project> Projects => Set<Project>();
        public override int SaveChanges()
        {
            ApplyAuditInfo();
            return base.SaveChanges();
        }

        private void ApplyAuditInfo()
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.CreatedBy = _currentUserService.UserId;
                        entry.Entity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        entry.Entity.UpdatedBy = _currentUserService.UserId;
                        break;
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var propertyMethodInfo = typeof(EF).GetMethod("Property")?.MakeGenericMethod(typeof(bool));
                    var isDeletedProperty = Expression.Call(null, propertyMethodInfo!, parameter, Expression.Constant("IsDeleted"));
                    var compareExpression = Expression.MakeBinary(ExpressionType.Equal, isDeletedProperty, Expression.Constant(false));
                    var lambda = Expression.Lambda(compareExpression, parameter);

                    builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
            builder.Entity<TaskDependency>()
                   .HasOne(td => td.Task)
                   .WithMany(t => t.Dependencies)
                   .HasForeignKey(td => td.TaskId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TaskDependency>()
                .HasOne(td => td.DependsOnTask)
                .WithMany(t => t.DependentTasks)
                .HasForeignKey(td => td.DependsOnTaskId)
                .OnDelete(DeleteBehavior.Restrict);

       
            builder.Entity<TaskItem>(entity =>
            {
                entity.HasOne(t => t.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(t => t.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict); 

                entity.HasOne(t => t.AssignedToUser)
                    .WithMany()
                    .HasForeignKey(t => t.AssignedToUserId)
                    .OnDelete(DeleteBehavior.Restrict); 

                entity.HasOne(t => t.ReviewedByUser)
                    .WithMany()
                    .HasForeignKey(t => t.ReviewedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            builder.ApplyConfigurationsFromAssembly(typeof(DatabaseDbContext).Assembly);
        }
    }
}