
// Yevgeniy Mazur
// User registration and account details.
// Updated 11/15/2026
// Added an administrator-only user list endpoint to support
// customer selection in the Music Rental module.

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibeHive.Api.Events;
using VibeHive.Contracts;

namespace VibeHive.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(EventStore store) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public ActionResult<UserDto> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        var allowedRoles = new[]
        {
            UserRoles.User,
            UserRoles.Admin,
            UserRoles.EventManager
        };

        if (!allowedRoles.Contains(request.Role))
            return BadRequest("Select a valid role.");

        lock (store.Gate)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            if (store.Users.Any(account =>
                account.User.Email == email))
            {
                return Conflict("Email already registered.");
            }

            var account = new Account
            {
                User = new UserDto(
                    Guid.NewGuid(),
                    request.Name.Trim(),
                    email,
                    request.Role)
            };

            account.PasswordHash = store.Hasher.HashPassword(
                account,
                request.Password);

            store.Users.Add(account);

            return StatusCode(201, account.User);
        }
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<UserDto> Me()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        lock (store.Gate)
        {
            var account = store.Users.SingleOrDefault(
                account => account.User.Id.ToString() == userId);

            if (account is null)
                return NotFound("User not found.");

            return Ok(account.User);
        }
    }

    // GET /api/users
    // Allows administrators to select registered customers
    // when managing music rentals.
    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet]
    public ActionResult<IEnumerable<UserDto>> GetUsers()
    {
        lock (store.Gate)
        {
            var users = store.Users
                .Select(account => account.User)
                .OrderBy(user => user.Name)
                .ToList();

            return Ok(users);
        }
    }
}
