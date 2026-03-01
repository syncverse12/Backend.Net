using SyncVerse.API.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace SyncVerse.API.Authorization.Handlers
{
    public class ReviewTaskAuthorizationHandler
     : AuthorizationHandler<ReviewTaskRequirement, TaskItem>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ReviewTaskRequirement requirement,
            TaskItem task)
        {
            if (!context.User.IsInRole("Manager"))
                return Task.CompletedTask;

            if (task.Status == TaskStatus.Submitted)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
