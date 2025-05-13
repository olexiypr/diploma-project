using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Services.Topics.Services;

public class CustomAuthService : IAuthorizationService
{
    public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements)
    {
        throw new NotImplementedException();
    }

    public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
    {
        throw new NotImplementedException();
    }
}