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

        public DbSet<UserWorkspace> UserWorkspaces => Set<UserWorkspace>();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInfo();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public DbSet<TaskEmployee> TaskEmployees => Set<TaskEmployee>();
        public DbSet<TimeLog> TimeLogs => Set<TimeLog>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<Milestone> Milestones => Set<Milestone>();
        public DbSet<TaskComment> TaskComments => Set<TaskComment>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<ProjectInvitation> ProjectInvitations { get; set; }
        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
        public DbSet<ProjectAttachment> ProjectAttachments { get; set; }
        public DbSet<TaskAttachment> TaskAttachments { get; set; }
        public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
        public DbSet<Team> Teams => Set<Team>();
        public DbSet<CompanyInvitation> CompanyInvitations => Set<CompanyInvitation>();
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<TaskCategory> TaskCategories => Set<TaskCategory>();

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

            builder.Entity<User>().Ignore(u => u.WorkspaceId);
            builder.Entity<User>().Ignore(u => u.Workspace);
            builder.Entity<UserWorkspace>()
                .HasKey(uw => new { uw.UserId, uw.WorkspaceId });

            builder.Entity<UserWorkspace>()
                .HasOne(uw => uw.User)
                .WithMany(u => u.UserWorkspaces)
                .HasForeignKey(uw => uw.UserId);

            builder.Entity<UserWorkspace>()
                .HasOne(uw => uw.Workspace)
                .WithMany(w => w.UserWorkspaces)
                .HasForeignKey(uw => uw.WorkspaceId);


            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType) &&
                    !entityType.ClrType.Name.Contains("User") &&
                    !entityType.ClrType.Name.Contains("Role") &&
                    entityType.ClrType != typeof(UserWorkspace)) 
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var propertyMethodInfo = typeof(EF).GetMethod("Property")?.MakeGenericMethod(typeof(bool));
                    var isDeletedProperty = Expression.Call(null, propertyMethodInfo!, parameter, Expression.Constant("IsDeleted"));
                    var compareExpression = Expression.MakeBinary(ExpressionType.Equal, isDeletedProperty, Expression.Constant(false));
                    var lambda = Expression.Lambda(compareExpression, parameter);

                    builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }

            builder.Entity<UserSettings>().HasKey(us => us.UserId);
            builder.Entity<UserSettings>().HasOne(us => us.User).WithOne(u => u.Settings).HasForeignKey<UserSettings>(us => us.UserId);

            builder.ApplyConfigurationsFromAssembly(typeof(DatabaseDbContext).Assembly);


            builder.Entity<TaskCategory>().ToTable("TaskCategories");
            builder.Entity<Workspace>().HasOne(w => w.CreatedByUser).WithMany().HasForeignKey(w => w.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Team>().HasOne(t => t.Workspace).WithMany().HasForeignKey(t => t.WorkspaceId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<TeamMember>().HasOne(tm => tm.Team).WithMany(t => t.TeamMembers).HasForeignKey(tm => tm.TeamId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<TaskItem>().HasOne(t => t.Workspace).WithMany().HasForeignKey(t => t.WorkspaceId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Notification>().HasOne(n => n.Workspace).WithMany().HasForeignKey(n => n.WorkspaceId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserWorkspace>()
                .HasQueryFilter(uw => !uw.IsDeleted && !uw.Workspace.IsDeleted);
        }
    }
}