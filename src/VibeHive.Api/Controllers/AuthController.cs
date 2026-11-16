// Yevgeniy Mazur
// Login and JWT authentication.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VibeHive.Api.Events;
using VibeHive.Contracts;

namespace VibeHive.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    EventStore store,
    TokenService tokens) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public ActionResult<AuthResponse> Login(LoginRequest request)
    {
        lock (store.Gate)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var account = store.Users.SingleOrDefault(
                account => account.User.Email == email);

            if (account is null)
                return Unauthorized("Invalid email or password.");

            var result = store.Hasher.VerifyHashedPassword(
                account,
                account.PasswordHash,
                request.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid email or password.");

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                account.PasswordHash = store.Hasher.HashPassword(
                    account,
                    request.Password);
            }

            return Ok(tokens.Issue(account.User));
        }
    }
}
