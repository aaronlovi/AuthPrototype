using System;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.RateLimit;
using Polly.Retry;

namespace OAuthToolkit.Shared;

public static class PollyExtensions {
    public static Context WithLogger(this Context context, ILogger logger) {
        context["logger"] = logger;
        return context;
    }

    public static AsyncRetryPolicy<T> GetRetryPolicy<T>(
        int maxRetryAttempts,
        TimeSpan[] retryDelays,
        Func<T, bool> resultPredicate) {
        return Policy
            .HandleResult(resultPredicate)
            .WaitAndRetryAsync(maxRetryAttempts, retryAttempt => retryDelays[retryAttempt - 1],
                (result, timeSpan, retryCount, context) => {
                    var logger = (ILogger)context["logger"];
                    logger.LogWarning("Operation - retry {RetryCount} after {Delay} due to result: {Result}", retryCount, timeSpan, result.Result);
                });
    }

    public static AsyncCircuitBreakerPolicy<T> GetCircuitBreakerPolicy<T>(
        int maxExceptionsBeforeBreak,
        TimeSpan durationOfBreak, 
        Func<T, bool> resultPredicate) {
        return Policy
            .HandleResult(resultPredicate)
            .CircuitBreakerAsync(maxExceptionsBeforeBreak, durationOfBreak,
                onBreak: (result, breakDelay, context) => {
                    var logger = (ILogger)context["logger"];
                    logger.LogWarning("Circuit breaker opened for {BreakDelay} due to result: {Result}", breakDelay, result.Result);
                },
                onReset: context => {
                    var logger = (ILogger)context["logger"];
                    logger.LogInformation("Circuit breaker reset");
                });
    }

    public static AsyncRateLimitPolicy<T> GetRateLimitPolicy<T>(int numberOfExecutions, TimeSpan perTimeSpan) =>
        Policy.RateLimitAsync<T>(numberOfExecutions, perTimeSpan, numberOfExecutions);
}
