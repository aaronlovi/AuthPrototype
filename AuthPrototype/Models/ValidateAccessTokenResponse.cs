using System;

namespace AuthPrototype.Models;

public record ValidateAccessTokenResponse(bool IsValid, DateTime? ExpirationTime)
{
    public static readonly ValidateAccessTokenResponse Invalid = new(false, null);

    public bool IsInvalid => this == Invalid || IsValid == false || ExpirationTime is null;
}
