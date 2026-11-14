// Yevgeniy Mazur
// Registration and login.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VibeHive.Api.Events;
using VibeHive.Contracts;
namespace VibeHive.Api.Controllers;
[ApiController, Route("api/auth")]
public class AuthController(EventStore store, TokenService tokens) : ControllerBase
{
    [AllowAnonymous, HttpPost("register")]
    public ActionResult<UserDto> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Name is required.");
        lock (store.Gate)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            if (store.Users.Any(u => u.User.Email == email)) return Conflict("Email already registered.");
            var account = new Account { User = new(Guid.NewGuid(), request.Name.Trim(), email, "Customer") };
            account.PasswordHash = store.Hasher.HashPassword(account, request.Password);
            store.Users.Add(account);
            return StatusCode(201, account.User);
        }
    }
    [AllowAnonymous, HttpPost("login")]
    public ActionResult<AuthResponse> Login(LoginRequest request)
    {
        lock (store.Gate)
        {
            var account = store.Users.SingleOrDefault(u => u.User.Email == request.Email.Trim().ToLowerInvariant());
            if (account is null) return Unauthorized("Invalid email or password.");
            var result = store.Hasher.VerifyHashedPassword(account, account.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed) return Unauthorized("Invalid email or password.");
            if (result == PasswordVerificationResult.SuccessRehashNeeded) account.PasswordHash = store.Hasher.HashPassword(account, request.Password);
            return tokens.Issue(account.User);
        }
    }
}
