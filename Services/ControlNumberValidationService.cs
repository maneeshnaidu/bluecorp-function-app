using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bluecorp_function_app.Interfaces;
using StackExchange.Redis;

namespace bluecorp_function_app.Services
{
    public class ControlNumberValidationService : IControlNumberValidationService
    {
        private readonly IDatabase _redisDb;

        // Constructor injection for Redis database instance
        public ControlNumberValidationService(IConnectionMultiplexer redisConnection)
        {
            _redisDb = redisConnection.GetDatabase();
        }

        public async Task<bool> IsControlNumberIncrementedAsync(int controlNumber)
        {
            // Get the last cached control number from Redis
            var lastControlNumber = await _redisDb.StringGetAsync("lastControlNumber");

            if (lastControlNumber.IsNullOrEmpty)
            {
                // If no control number is found in Redis, consider this the first valid one
                return true;
            }

            // Compare the incoming control number with the last stored one
            int lastStoredControlNumber = (int)lastControlNumber;

            // If the new control number is greater, it's considered valid
            return controlNumber > lastStoredControlNumber;
        }

        public async Task StoreControlNumberAsync(int controlNumber)
        {
            // Store the new control number in Redis
            await _redisDb.StringSetAsync("lastControlNumber", controlNumber);
        }
    }
}