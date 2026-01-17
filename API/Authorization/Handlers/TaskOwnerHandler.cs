using Graduation_Project.Application.Interfaces.Persistence;
using Graduation_Project.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public class TaskOwnerHandler
    : AuthorizationHandler<TaskOwnerRequirement, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public TaskOwnerHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TaskOwnerRequirement requirement,
        int taskId)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        var task = await _unitOfWork.Repository<TaskItem>()
            .GetByIdAsync(taskId);

        if (task == null) return;

        if (task.CreatedByUserId == userId)
            context.Succeed(requirement);
    }
}
