using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Host.TestApps.IntegrationTests.Specs.Glue.OidcProxyNet;

public class DummyClaimHandler : AuthorizationHandler<DummyClaimRequirement>
{
    public static IEnumerable<Claim> Claims = Array.Empty<Claim>();
    
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DummyClaimRequirement requirement)
    {
        Claims = context.User.Claims;
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}