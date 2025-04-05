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
    public async Task<IActionResult> Authenticate([FromBody] OAuthTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.AccessToken) || string.IsNullOrEmpty(request.RefreshToken))
            return BadRequest("Invalid request");

        var isValidToken = await _tokenService.ValidateAccessTokenAsync(request.AccessToken);
        if (!isValidToken)
            return Unauthorized("Invalid access token");

        var user = new User(Guid.NewGuid().ToString(), request.Name, request.Email, "Google", request.AccessToken, request.RefreshToken);
        _userStore.AddUser(user);

        return Ok("User authenticated and tokens stored");
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenRefreshRequest request)
    {
        // Validate the request
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.RefreshToken))
            return BadRequest("Invalid request");

        // Find the user by email
        var users = _userStore.GetAllUsers();
        var user = users.FirstOrDefault(u => u.Email == request.Email);

        if (user is null || user.RefreshToken != request.RefreshToken)
            return Unauthorized("Invalid refresh token");

        var (newAccessToken, newRefreshToken) = await _tokenService.RefreshTokenAsync(request.RefreshToken);

        // Update the user with new tokens
        var updatedUser = user with { AccessToken = newAccessToken, RefreshToken = newRefreshToken };
        _userStore.AddUser(updatedUser);

        return Ok("Tokens refreshed");
    }
}
