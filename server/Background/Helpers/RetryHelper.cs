using Npgsql;

namespace server.Background.Helpers;

public static class RetryHelper
{
   
    public static async Task RetryOnExceptionAsync(
        int maxRetries,
        Func<TimeSpan, int, TimeSpan> backoffStrategy,
        Func<Task> operation,
        Func<Exception, bool> isTransient,
        ILogger logger)
    {
        int attempt = 0;
        while (true)
        {
            try
            {
                await operation();
                return; // success
            }
            catch (Exception ex) when (isTransient(ex))
            {
                attempt++;
                if (attempt > maxRetries)
                {
                    logger.LogError(ex, "Operation failed after {Attempts} retries", attempt - 1);
                    throw;
                }

                var delay = backoffStrategy(TimeSpan.FromMilliseconds(200), attempt);
                logger.LogWarning(ex, "Transient error, retrying in {Delay}ms (attempt {Attempt})", delay.TotalMilliseconds, attempt);
                await Task.Delay(delay);
            }
        }
    }
}

public static class NpgsqlExceptionExtensions
{
    public static bool IsTransient(this NpgsqlException ex) =>
        ex.SqlState == "57P01" // admin shutdown
        || ex.SqlState == "53300" // too many connections
        || ex.SqlState == "08006"; // connection failure
}

public static class Backoff
{
    public static TimeSpan LinearBackoff(TimeSpan baseDelay, int attempt) => 
    TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * attempt);
}

