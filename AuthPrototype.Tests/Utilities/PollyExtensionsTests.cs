using AuthPrototype.Utilities;
using Polly.CircuitBreaker;
using Polly;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Polly.RateLimit;

namespace AuthPrototype.Tests.Utilities;

public class PollyExtensionsTests {
    private readonly Mock<ILogger> _mockLogger;

    public PollyExtensionsTests() {
        _mockLogger = new Mock<ILogger>();
    }

    [Fact]
    public async Task GetRetryPolicy_ShouldRetry_OnFailure() {
        var retryPolicy = PollyExtensions.GetRetryPolicy<HttpResponseMessage>(
            maxRetryAttempts: 3,
            retryDelays: [TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3)],
            resultPredicate: r => !r.IsSuccessStatusCode);

        var context = new Context().WithLogger(_mockLogger.Object);
        var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
        var attempts = 0;

        await retryPolicy.ExecuteAsync(async ctx => {
            attempts++;
            return await Task.FromResult(httpResponse);
        }, context);

        Assert.Equal(4, attempts); // Initial attempt + 3 retries
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Exactly(3)); // 3 retries logged
    }

    [Fact]
    public async Task GetRetryPolicy_ShouldNotRetry_OnSuccess() {
        var retryPolicy = PollyExtensions.GetRetryPolicy<HttpResponseMessage>(
            maxRetryAttempts: 3,
            retryDelays: [ TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3) ],
            resultPredicate: r => !r.IsSuccessStatusCode);

        var context = new Context().WithLogger(_mockLogger.Object);
        var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        var attempts = 0;

        await retryPolicy.ExecuteAsync(async ctx =>
        {
            attempts++;
            return await Task.FromResult(httpResponse);
        }, context);

        Assert.Equal(1, attempts); // Only initial attempt, no retries
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Never); // No retries logged
    }

    [Fact]
    public async Task GetCircuitBreakerPolicy_ShouldOpenCircuit_OnConsecutiveFailures() {
        var circuitBreakerPolicy = PollyExtensions.GetCircuitBreakerPolicy<HttpResponseMessage>(
            maxExceptionsBeforeBreak: 2,
            durationOfBreak: TimeSpan.FromMinutes(1),
            resultPredicate: r => !r.IsSuccessStatusCode);

        var context = new Context().WithLogger(_mockLogger.Object);
        var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);

        await Assert.ThrowsAsync<BrokenCircuitException<HttpResponseMessage>>(async () => {
            for (int i = 0; i < 3; i++) {
                await circuitBreakerPolicy.ExecuteAsync(async ctx => {
                    return await Task.FromResult(httpResponse);
                }, context);
            }
        });

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.AtLeastOnce); // Circuit breaker logs
    }

    [Fact]
    public async Task GetCircuitBreakerPolicy_ShouldNotOpenCircuit_OnSuccess() {
        var circuitBreakerPolicy = PollyExtensions.GetCircuitBreakerPolicy<HttpResponseMessage>(
            maxExceptionsBeforeBreak: 2,
            durationOfBreak: TimeSpan.FromMinutes(1),
            resultPredicate: r => !r.IsSuccessStatusCode);

        var context = new Context().WithLogger(_mockLogger.Object);
        var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

        for (int i = 0; i < 3; i++) {
            await circuitBreakerPolicy.ExecuteAsync(async ctx =>
            {
                return await Task.FromResult(httpResponse);
            }, context);
        }

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Never); // No circuit breaker logs
    }

    [Fact]
    public async Task GetRateLimitPolicy_ShouldLimitExecutions() {
        for (int i = 0; i < 1000; i++) {
            var rateLimitPolicy = PollyExtensions.GetRateLimitPolicy<HttpResponseMessage>(
                numberOfExecutions: 2,
                perTimeSpan: TimeSpan.FromSeconds(10));

            var context = new Context().WithLogger(_mockLogger.Object);
            var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

            await rateLimitPolicy.ExecuteAsync(async ctx => await Task.FromResult(httpResponse), context);
            await rateLimitPolicy.ExecuteAsync(async ctx => await Task.FromResult(httpResponse), context);

            RateLimitRejectedException exception = await Assert.ThrowsAsync<RateLimitRejectedException>(async () => {
                await rateLimitPolicy.ExecuteAsync(async ctx => {
                    return await Task.FromResult(httpResponse);
                }, context);
            });

            Assert.True(exception.RetryAfter > TimeSpan.Zero);
        }
    }

    [Fact]
    public void WithLogger_ShouldAddLoggerToContext() {
        var context = new Context();

        context = context.WithLogger(_mockLogger.Object);

        Assert.True(context.ContainsKey("logger"));
        Assert.Equal(_mockLogger.Object, context["logger"]);
    }

    [Fact]
    public void WithLogger_ShouldHandleNullLogger() {
        var context = new Context();

        context = context.WithLogger(null!);

        Assert.True(context.ContainsKey("logger"));
        Assert.Null(context["logger"]);
    }
}
