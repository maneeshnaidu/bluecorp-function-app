using Polly;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using bluecorp_function_app.Interfaces;

namespace bluecorp_function_app.Services
{
    public class HttpRetryService : IHttpRetryService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task ExecuteWithRetryAsync(Func<Task> action)
        {
            // Define the retry policy using Polly
            var retryPolicy = Policy
                .Handle<HttpRequestException>() // Retry on HttpRequestException
                .WaitAndRetryAsync(3, // Retry up to 3 times
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff: 1s, 2s, 4s...
                    (exception, timeSpan, retryCount, context) =>
                    {
                        // Log retry attempts if needed (e.g., with a logger or telemetry)
                        Console.WriteLine($"Retry {retryCount} due to: {exception.Message}. Retrying in {timeSpan.TotalSeconds} seconds...");
                    });

            // Wrap your action (HTTP request, for example) with the retry policy
            await retryPolicy.ExecuteAsync(action);
        }
    }
}