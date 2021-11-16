using System;
using StackExchange.Redis;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using OpenTelemetry.Trace;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Contrib.Extensions.AWSXRay.Trace;
using System.Net;

namespace RedisDotnetSample
{
    public class RedisConnectorHelper
    {
        static private string cacheConnection = System.Environment.GetEnvironmentVariable("REDIS_ENDPOINT");


        static RedisConnectorHelper()
        {
            RedisConnectorHelper._connection = new Lazy<ConnectionMultiplexer>(() =>
            {
                var connection = ConnectionMultiplexer.Connect(cacheConnection + ",allowAdmin=true");
                var tracerProvider = Sdk.CreateTracerProviderBuilder()
                    // .AddXRayTraceId()
                    // .AddConsoleExporter()
                    // .AddAWSInstrumentation()
                    .AddRedisInstrumentation(connection,options => options.SetVerboseDatabaseStatements = true)
                    .Build();
                return connection;
            });
        }

        private static Lazy<ConnectionMultiplexer> _connection;

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return _connection.Value;
            }
        }
    }
    class Program
    {
        static private bool dbFlush = System.Environment.GetEnvironmentVariable("DBFLUSH") == "TRUE";
        static private bool stressMode = System.Environment.GetEnvironmentVariable("STRESS_MODE") == "ON";

        static void FlushDatabase(){
            System.Net.EndPoint[] endpoints = RedisConnectorHelper.Connection.GetEndPoints();
            foreach (EndPoint e in endpoints){
                var server = RedisConnectorHelper.Connection.GetServer(e);
                if (! server.IsReplica) server.FlushDatabase();

            }

        }


        static void WriteDataToRedis(int count)
        {
            IDatabase cache = RedisConnectorHelper.Connection.GetDatabase();

            Stopwatch stopWatch = new Stopwatch();
            for (int i = 0; i < count; i++)
            {
                string GUID = Guid.NewGuid().ToString();
                stopWatch.Start();
                cache.StringSet(GUID, LoremIpsum(20, 40, 2, 3, 1));
                Console.WriteLine("Key: " + GUID + ", Value: " + cache.StringGet(GUID, CommandFlags.PreferReplica).ToString());
                stopWatch.Stop();
                Console.WriteLine("Time Elapsed for 1 write and 1 read: " + stopWatch.ElapsedMilliseconds.ToString() + " ms");
                stopWatch.Restart();
            }
        }

        private static async Task WriteDataToRedisAysnc(int count)
        {
            IDatabase cache = RedisConnectorHelper.Connection.GetDatabase();
            Task<bool>[] RandomSetStringTask = new Task<bool>[count];

            for (int i = 0; i < count; i++)
            {
                // Stopwatch stopWatch = new Stopwatch();
                // stopWatch.Start();
                string GUID = Guid.NewGuid().ToString();
                Console.WriteLine(String.Format("Calling aync method {0}", i));
                RandomSetStringTask[i] = cache.StringSetAsync(GUID, LoremIpsum(20, 40, 2, 3, 3));
            }
            await Task.WhenAll(RandomSetStringTask);
            return;
        }

        private static void FireAndForget()
        {
            IDatabase cache = RedisConnectorHelper.Connection.GetDatabase();

            // Fire-and-Forget
            string pageKey = "/catalog/49865/";
            for (int i = 0; i < 500; i++)
            {
                cache.StringIncrement(pageKey, flags: CommandFlags.FireAndForget);
                Console.WriteLine("Page View +1");
            }
        }

        async static Task Main(string[] args)
        {
            // FlushDatabase();

            WriteDataToRedis(100);

            if (stressMode)
            {
                for (int i = 0;; i++) WriteDataToRedis(10000);
            }

            Parallel.For(1, 10, i => WriteDataToRedis(100));

            // Async mode
            for (int i = 0; i < 100; i++)
            {
                await WriteDataToRedisAysnc(5);
            }

            IDatabase cache = RedisConnectorHelper.Connection.GetDatabase();

            FireAndForget();

            // Decrement 
            cache.StringSet("coffee", "90000");
            for (int i = 0; i < 2000; i++) { cache.StringDecrement("coffee"); }
            Console.WriteLine(String.Format("coffee count is now {0}", cache.StringGet("coffee")));

            // Simple PING command
            string cacheCommand = "PING";
            Console.WriteLine("\nCache command  : " + cacheCommand);
            Console.WriteLine("Cache response : " + cache.Execute(cacheCommand).ToString());

            // SortSet
            cache.SortedSetAdd("apple", "iphone:13:128G", 200);
            cache.SortedSetAdd("apple", "iphone:13:256G", 400);
            cache.SortedSetAdd("apple", "iphone:13:256G:plus", 100);

            var collection = cache.SortedSetRangeByRankWithScores("apple", 0, -1, Order.Descending);
            foreach (var item in collection)
            {
                Console.WriteLine($"{item.Element} {item.Score}");
            }

            // Hashes 
            cache.HashSet("username:1122", "name", "Darren Lin");
            cache.HashSet("username:1122", "birthday", "2000/1/1");
            cache.HashSet("username:1122", "address", "Taipei City");

            Dictionary<string, string> responseDict = cache.HashGetAll("username:1122").ToStringDictionary();

            foreach (var item in responseDict)
            {
                Console.WriteLine($"{item.Key} : {item.Value}");
            }

            // Set 
            cache.SetAdd("myset", "Red");
            cache.SetAdd("myset", "Blue");
            cache.SetAdd("myset", "Green");
            String[] response = cache.SetMembers("myset").ToStringArray();
            foreach (String resp in response) { Console.WriteLine(resp); }

            // Lists
            cache.ListLeftPush("mylist", "Apple");
            cache.ListLeftPush("mylist", "Banana");
            cache.ListLeftPush("mylist", "Orange");
            response = cache.ListRange("mylist", 0, -1).ToStringArray();
            foreach (String resp in response) { Console.WriteLine(resp); }

            // Trying to do MGET MSET...

            /* 
                Sending the message to the Redis Server will return "(error) CROSSSLOT Keys in request don't hash to the same slot"

                darren-demo.lm5w0w.clustercfg.usw2.cache.amazonaws.com:6379> MSET a b c d
                (error) CROSSSLOT Keys in request don't hash to the same slot
            */
            cacheCommand = "MSET aaa bbbb cccc dddd";
            try
            {
                Console.WriteLine("Cache response : " + cache.Execute(cacheCommand).ToString());
            }
            catch (StackExchange.Redis.RedisServerException e)
            {
                Console.WriteLine("Failed to do MSET on a Redis Cluster!");
                Console.WriteLine(e.ToString());
            }
            catch (System.Exception)
            {
                throw;
            }

            // Writing Messages
            cacheCommand = "SET Message \"Hello! The cache is working from a .NET Core console app!\"";
            Console.WriteLine("\nCache command  : " + cacheCommand + " or StringSet()");
            Console.WriteLine("Cache response : " + cache.StringSet("Message", "Hello! The cache is working from a .NET Core console app!").ToString());

            // Reading from Read Replicas
            // Simple get and put of integral data types into the cache
            cacheCommand = "GET Message";
            Console.WriteLine("\nCache command  : " + cacheCommand + " or StringGet()");
            Console.WriteLine("Cache response : " + cache.StringGet("Message").ToString());


            for (int i = 0; i < 50; i++)
            {
                cache.StringGet("Message", CommandFlags.PreferReplica);
                // Thread.Sleep(1000);
                if (i % 10 == 0) Console.WriteLine("Reading Message from Redis...");
            }

        }



        static string LoremIpsum(int minWords, int maxWords, int minSentences, int maxSentences, int numParagraphs)
        {

            var words = new[] { "lorem", "ipsum", "dolor", "sit", "amet", "consectetuer", "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod", "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat" };
            var rand = new Random();
            int numSentences = rand.Next(maxSentences - minSentences) + minSentences + 1;
            int numWords = rand.Next(maxWords - minWords) + minWords + 1;

            StringBuilder result = new StringBuilder();

            for (int p = 0; p < numParagraphs; p++)
            {
                result.Append("---");
                for (int s = 0; s < numSentences; s++)
                {
                    for (int w = 0; w < numWords; w++)
                    {
                        if (w > 0) { result.Append(" "); }
                        result.Append(words[rand.Next(words.Length)]);
                    }
                    result.Append(". ");
                }
                result.Append("---");
            }

            return result.ToString();
        }

    }

}
