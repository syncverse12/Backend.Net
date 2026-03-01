using SyncVerse.API.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace SyncVerse.API.Authorization.Handlers
{
    public class ManagerAuthorizationHandler
      : AuthorizationHandler<ManagerRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ManagerRequirement requirement)
        {
            if (context.User.IsInRole("Manager"))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

}
