using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DigitalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected Guid? GetUserId()
        {
            var userIdStr = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdStr, out var userId) ? userId : null;
        }
    }
}
