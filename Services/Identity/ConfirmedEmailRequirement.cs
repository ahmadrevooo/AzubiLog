using AzubiLog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AzubiLog.Services.Identity;

public sealed class ConfirmedEmailRequirement : IAuthorizationRequirement;

public sealed class ConfirmedEmailHandler(UserManager<ApplicationUser> userManager)
    : AuthorizationHandler<ConfirmedEmailRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ConfirmedEmailRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var user = await userManager.GetUserAsync(context.User);
        if (user is not null)
        {
            context.Succeed(requirement);
        }
    }
}
