using System;
using System.Linq;
using System.Threading.Tasks;
using AuthPrototype.Models;
using AuthPrototype.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthPrototype.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UsersFileStore _userStore;
    private readonly TokenService _tokenService;

    public AuthController(UsersFileStore userStore, TokenService tokenService)
    {
        _userStore = userStore;
        _tokenService = tokenService;
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.AccessToken))
            return BadRequest("Invalid request");

        ValidateAccessTokenResponse validationResponse = await _tokenService.ValidateAccessToken(request.AccessToken);
        if (validationResponse.IsInvalid)
            return Unauthorized("Invalid access token");

        DateTime expirationTime = validationResponse.ExpirationTime!.Value;

        var user = new User(Guid.NewGuid().ToString(), request.Name, request.Email, "Google", validationResponse.ProviderUserId, request.AccessToken, expirationTime);
        _userStore.AddOrUpdateUser(user);

        return Ok(new AuthenticateResponse(request.AccessToken, expirationTime));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenRefreshRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.AccessToken))
            return BadRequest("Invalid request");

        var users = _userStore.GetAllUsers();
        var user = users.FirstOrDefault(u => u.Email == request.Email);

        if (user is null)
            return Unauthorized("User not found");

        ValidateAccessTokenResponse validationResponse = await _tokenService.ValidateAccessToken(request.AccessToken);
        if (validationResponse.IsInvalid || validationResponse.ExpirationTime!.Value <= DateTime.UtcNow)
            return Unauthorized("Access token expired");

        DateTime expirationTime = validationResponse.ExpirationTime!.Value;

        // Update the user with new tokens and expiration date/time
        var updatedUser = user with { AccessToken = request.AccessToken, ExpirationDateTime = expirationTime };
        _userStore.AddOrUpdateUser(updatedUser);

        return Ok(new TokenRefreshResponse(user.AccessToken, expirationTime));
    }

    [HttpPost("signout")]
    public IActionResult SignOut([FromBody] SignOutRequest request)
    {
        if (string.IsNullOrEmpty(request.Email))
            return BadRequest("Invalid request");

        _userStore.RemoveUser(request.Email);

        return Ok("User signed out successfully");
    }
}
