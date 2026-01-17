using Graduation_Project.Application.Interfaces.Persistence;
using Graduation_Project.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public class TaskOwnerAuthorizationHandler
    : AuthorizationHandler<TaskOwnerRequirement, TaskItem>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TaskOwnerRequirement requirement,
        TaskItem task)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (task.CreatedByUserId == userId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}