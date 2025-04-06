using System;

namespace AuthPrototype.Models;

/// <summary>
/// Represents a user authenticated through an external provider.
/// </summary>
/// <param name="SessionId">The session id within this backend</param>
/// <param name="Name">User's name (provided by client)</param>
/// <param name="Email">User's email for the provider</param>
/// <param name="Provider">E.g., Google</param>
/// <param name="ProviderUserId">Unique id for the user for that provider</param>
/// <param name="AccessToken">Access token given by the provider</param>
/// <param name="ExpirationDateTime">Expiration date for the token, according to the provider</param>
public record User(
    string SessionId,
    string Name,
    string Email,
    string Provider,
    string ProviderUserId,
    string AccessToken,
    DateTime ExpirationDateTime);
