using LibraryManagement.WebAPI.Services.Interfaces;
using System.Security.Claims;

namespace LibraryManagement.WebAPI.Services.Implementations;
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _contextAccessor;
    public CurrentUserService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
    }
    public Guid UserId()
    {
        try
        {
            var user = _contextAccessor.HttpContext?.User;

            var userIdClaim =
                user?.FindFirst(ClaimTypes.NameIdentifier) ??
                user?.FindFirst("sub");

            if (userIdClaim == null)
                throw new InvalidOperationException("User is not authenticated or claim missing.");

            return Guid.Parse(userIdClaim.Value);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(ex.Message);
        }
    }
}

