// Yevgeniy Mazur
// Current user details.

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibeHive.Api.Events;
using VibeHive.Contracts;
namespace VibeHive.Api.Controllers;
[ApiController, Authorize, Route("api/users")]
public class UsersController(EventStore store) : ControllerBase
{
    [HttpGet("me")]
    public ActionResult<UserDto> Me()
    {
        lock (store.Gate)
        {
            var account = store.Users.SingleOrDefault(u => u.User.Id.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier));
            return account is null ? NotFound() : Ok(account.User);
        }
    }
}
