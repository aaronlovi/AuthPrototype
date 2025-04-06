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

        ValidateAccessTokenResponse validationResponse = await _tokenService.ValidateAccessTokenAsync(request.AccessToken);
        if (validationResponse.IsInvalid)
            return Unauthorized("Invalid access token");

        DateTime expirationTime = validationResponse.ExpirationTime!.Value;

        var user = new User(Guid.NewGuid().ToString(), request.Name, request.Email, "Google", request.AccessToken, expirationTime);
        _userStore.AddOrUpdateUser(user);

        return Ok(new AuthenticateResponse(request.AccessToken, expirationTime));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenRefreshRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.AccessToken) || string.IsNullOrEmpty(request.RefreshToken))
            return BadRequest("Invalid request");

        var users = _userStore.GetAllUsers();
        var user = users.FirstOrDefault(u => u.Email == request.Email);

        if (user is null)
            return Unauthorized("User not found");

        var (isValidToken, expirationDateTime) = await _tokenService.ValidateAccessTokenAsync(request.AccessToken);
        if (!isValidToken || expirationDateTime is null || expirationDateTime <= DateTime.UtcNow)
            return Unauthorized("Access token expired");

        // Update the user with new tokens and expiration date/time
        var updatedUser = user with { AccessToken = request.AccessToken, ExpirationDateTime = expirationDateTime.Value };
        _userStore.AddOrUpdateUser(updatedUser);

        return Ok(new { user.AccessToken, ExpirationDateTime = expirationDateTime.Value.ToString("o") });
    }
}
