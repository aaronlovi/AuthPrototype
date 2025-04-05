using System;

namespace AuthPrototype.Models;

public record User(
    string Id,
    string Name,
    string Email,
    string Provider,
    string AccessToken,
    string RefreshToken,
    DateTime ExpirationDateTime);
