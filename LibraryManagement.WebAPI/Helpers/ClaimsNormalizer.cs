using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace LibraryManagement.WebAPI.Helpers;

public class ClaimsNormalizer : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = (ClaimsIdentity)principal.Identity!;

        // Normalize User ID
        var userId =
            identity.FindFirst("oid")?.Value ??
            identity.FindFirst("sub")?.Value;

        if (userId != null && !identity.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
        {
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
        }

        // Normalize Roles
        var roles =
            identity.FindAll("roles").Select(c => c.Value)
            .Concat(identity.FindAll("role").Select(c => c.Value));

        foreach (var role in roles)
        {
            if (!identity.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == role))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        }

        return Task.FromResult(principal);
    }
}
