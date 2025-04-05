using System;

namespace AuthPrototype.Models;

public record TokenResponse(string AccessToken, string RefreshToken, int ExpiresIn);
