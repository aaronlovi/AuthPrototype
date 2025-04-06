using System;

namespace AuthPrototype.Models;

public record AuthenticateRequest(string Name, string Email, string AccessToken);

public record AuthenticateResponse(string AccessToken, DateTime ExpirationDateTime);
