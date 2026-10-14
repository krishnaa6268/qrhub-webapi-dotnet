using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using QRHub.Application.Common.Exceptions;

namespace QRHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected Guid CurrentUserId
    {
        get
        {
            var idString = User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(idString) || !Guid.TryParse(idString, out var userId))
            {
                throw new UnauthorizedException("User is not authenticated.");
            }

            return userId;
        }
    }
}
