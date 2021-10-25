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

                // Increment the value by 50
                long newValue = await db.StringIncrementAsync("counter", 50);
                Console.WriteLine($"INCR new value = {newValue}");

                // PING should respond with "PONG".
                var result = await db.ExecuteAsync("ping");
                Console.WriteLine($"PING = {result.Type} : {result}");

                // Store Object type using JSON Serialization
                // https://docs.microsoft.com/en-us/learn/modules/optimize-your-web-apps-with-redis/5-execute-redis-commands?pivots=csharp
                var stat = new GameStat("Soccer", new DateTime(1950, 7, 16), "FIFA World Cup", 
                    new[] { "Uruguay", "Brazil" },
                    new[] { ("Uruguay", 2), ("Brazil", 1) });

                string serializedValue = Newtonsoft.Json.JsonConvert.SerializeObject(stat);
                bool added = db.StringSet("event:1950-world-cup", serializedValue);

                var statValue = db.StringGet("event:1950-world-cup");
                stat = Newtonsoft.Json.JsonConvert.DeserializeObject<GameStat>(statValue.ToString());
                Console.WriteLine(stat.Sport); // displays "Soccer"

                // Execute "FLUSHDB" to clear the database values.
                // It should respond with "OK".
                result = await db.ExecuteAsync("flushdb");
                Console.WriteLine($"FLUSHDB = {result.Type} : {result}");


            }
        }
    }
}
