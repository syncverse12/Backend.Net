using Graduation_Project.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public class TaskOwnerHandler
    : AuthorizationHandler<TaskOwnerRequirement, TaskItem>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TaskOwnerRequirement requirement,
        TaskItem task)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (context.User.IsInRole("Manager") ||
            task.UserId == userId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
