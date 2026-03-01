using SyncVerse.Application.Interfaces.Identity;
using SyncVerse.Domain.Common;
using SyncVerse.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace SyncVerse.Infrastructure.Data
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
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<Milestone> Milestones => Set<Milestone>();
        public DbSet<TaskComment> TaskComments => Set<TaskComment>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<ProjectInvitation> ProjectInvitations { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }

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
            builder.ApplyConfigurationsFromAssembly(typeof(DatabaseDbContext).Assembly);
        }
    }
}