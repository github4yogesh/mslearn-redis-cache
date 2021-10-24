using System;
using Microsoft.Extensions.Configuration;
using System.IO;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace SportsStatsTracker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            string connectionString = config["CacheConnection"];

            // The connection to Azure Cache for Redis is managed by the ConnectionMultiplexer class. 
            // This class should be shared and reused throughout your client application. We do not 
            // want to create a new connection for each operation. Here, we are only going to use it 
            // in the Main method, but in a production application, it should be stored in a class 
            // field, or a singleton.
            using (var cache = ConnectionMultiplexer.Connect(connectionString))
            {
                IDatabase db = cache.GetDatabase();
                
                // Add a value to the cache
                bool setValue = await db.StringSetAsync("test", "100");
                Console.WriteLine($"SET: {setValue}");

                // Get a value from the cache
                string getValue = await db.StringGetAsync("test");
                Console.WriteLine($"GET: {getValue}");
            }
        }
    }
}
