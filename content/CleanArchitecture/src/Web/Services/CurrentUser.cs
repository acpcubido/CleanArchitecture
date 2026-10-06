using Cubido.Template.Application.Common.Interfaces;
using System.Security.Claims;

namespace Cubido.Template.Web.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : IUser
{
    public string? Id => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}
