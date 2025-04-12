using System;

namespace OAuthToolkit.Shared;

/// <summary>
/// Defines an interface for providing the current date and time,
/// allowing for abstraction of DateTime operations for testability.
/// </summary>
public interface IDateTimeProvider {
    /// <summary>
    /// Gets the current date and time in UTC.
    /// </summary>
    DateTime NowUtc { get; }

    /// <summary>
    /// Gets the current date and time in local time.
    /// </summary>
    DateTime NowLocal { get; }
}

/// <summary>
/// Default implementation of <see cref="IDateTimeProvider"/> that uses
/// the system clock to provide current date and time values.
/// </summary>
public class DateTimeProvider : IDateTimeProvider {
    /// <summary>
    /// Gets the current date and time in UTC.
    /// </summary>
    /// <returns>A DateTime representing the current UTC date and time.</returns>
    public DateTime NowUtc => DateTime.UtcNow;

    /// <summary>
    /// Gets the current date and time in local time.
    /// </summary>
    /// <returns>A DateTime representing the current local date and time.</returns>
    public DateTime NowLocal => DateTime.Now;
}

/// <summary>
/// A test implementation of <see cref="IDateTimeProvider"/> that allows for
/// manual control of the current time.
/// </summary>
/// <remarks>
/// This implementation is intended for unit testing scenarios where
/// precise control over time is required. The internal representation of time
/// is always stored in UTC.
/// </remarks>
public class TestDateTimeProvider : IDateTimeProvider {
    private DateTime _theTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestDateTimeProvider"/> class.
    /// </summary>
    /// <param name="dt">
    /// Optional initial date and time value. If not specified, the current UTC time is used.
    /// </param>
    public TestDateTimeProvider(DateTime? dt = null) {
        _theTime = dt?.ToUniversalTime() ?? DateTime.UtcNow;
    }

    /// <summary>
    /// Gets or sets the current date and time in UTC.
    /// </summary>
    /// <remarks>
    /// When setting this property, the provided <see cref="DateTime"/> is automatically
    /// converted to UTC using <see cref="DateTime.ToUniversalTime"/> if it is not already in UTC.
    /// This ensures that the internal time is always stored in UTC.
    /// </remarks>
    public DateTime NowUtc {
        get => _theTime;
        set => _theTime = value.ToUniversalTime();
    }

    /// <summary>
    /// Gets or sets the current date and time in local time.
    /// </summary>
    /// <remarks>
    /// When getting this property, the internal UTC time is converted to local time.
    /// When setting this property, the provided <see cref="DateTime"/> is automatically
    /// converted to UTC for internal storage, regardless of its original <see cref="DateTimeKind"/>.
    /// </remarks>
    public DateTime NowLocal {
        get => _theTime.ToLocalTime();
        set => _theTime = value.ToUniversalTime();
    }
}
