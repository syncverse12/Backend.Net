using Microsoft.AspNetCore.Authorization;

namespace SyncVerse.API.Extensions
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly",
                    policy => policy.RequireRole("Admin", "HR"));

                options.AddPolicy("ManagerOnly",
                    policy => policy.RequireRole("Manager"));

                options.AddPolicy("AdminOrManager",
                    policy => policy.RequireRole("Admin", "Manager"));
            });

            return services;
        }
    }
}
