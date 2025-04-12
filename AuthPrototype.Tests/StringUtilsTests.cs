using System;
using OAuthToolkit.Shared;
using Xunit;

namespace OAuthToolkit.Shared.Tests;

public class StringUtilsTests {
    [Fact]
    public void Truncate_ShouldThrowArgumentNullException_WhenStringIsNull() {
        string value = null!;
        int threshold = 5;
        Assert.Throws<ArgumentNullException>(() => value.Truncate(threshold));
    }

    [Fact]
    public void Truncate_ShouldReturnOriginalString_WhenStringLengthIsLessThanThreshold() {
        string value = "Hello";
        int threshold = 10;
        var result = value.Truncate(threshold);
        Assert.Equal(value, result);
    }

    [Fact]
    public void Truncate_ShouldReturnOriginalString_WhenStringLengthIsEqualToThreshold() {
        string value = "Hello";
        int threshold = 5;
        var result = value.Truncate(threshold);
        Assert.Equal(value, result);
    }

    [Fact]
    public void Truncate_ShouldReturnTruncatedString_WhenStringLengthIsGreaterThanThreshold() {
        string value = "Hello, World!";
        int threshold = 5;
        var result = value.Truncate(threshold);
        Assert.Equal("Hello", result);
    }
}
