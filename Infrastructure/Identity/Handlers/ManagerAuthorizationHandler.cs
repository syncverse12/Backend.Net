using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public class ManagerAuthorizationHandler
    : AuthorizationHandler<ManagerRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManagerRequirement requirement)
    {
        var role = context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (role == "Manager")
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
