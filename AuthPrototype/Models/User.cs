using System;

namespace AuthPrototype.Models;

public record User(
    string SessionId,
    string Name,
    string Email,
    string Provider,
    string AccessToken,
    DateTime ExpirationDateTime);
