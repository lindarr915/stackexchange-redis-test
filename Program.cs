using System;
using StackExchange.Redis;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace Redistest
{
    public class RedisConnectorHelper
    {
        static private string cacheConnection = System.Environment.GetEnvironmentVariable("REDIS_ENDPOINT");

        static RedisConnectorHelper()
        {
            RedisConnectorHelper._connection = new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect(cacheConnection);
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

        static void WriteDataToRedis()
        {
            IDatabase cache = RedisConnectorHelper.Connection.GetDatabase();

            for (int i = 0; i < 10000; i++)
            {
                string GUID = Guid.NewGuid().ToString();
                cache.StringSet(GUID, LoremIpsum(20, 40, 2, 3, 4));
                Console.WriteLine("Key: " + GUID + ", Value: " + cache.StringGet(GUID).ToString());
            }
        }


        static void Main(string[] args)
        {

            // Test t = new Test();  
            Thread[] tr = new Thread[10];

            for (int i = 0; i < 10; i++)
            {
                tr[i] = new Thread(new ThreadStart(WriteDataToRedis));
                tr[i].Name = String.Format("Working Thread: {0}", i);
            }
            //Start each thread  

            foreach (Thread x in tr) { x.Start(); }
            foreach (Thread x in tr) { x.Join(); }


            IDatabase cache = RedisConnectorHelper.Connection.GetDatabase();
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
                Console.WriteLine("Failed to do MGET on a Redis Cluster!");
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

            for (int i = 0; i < 200; i++)
            {
                cache.StringGet("Message", CommandFlags.PreferReplica);
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
                result.Append("<p>");
                for (int s = 0; s < numSentences; s++)
                {
                    for (int w = 0; w < numWords; w++)
                    {
                        if (w > 0) { result.Append(" "); }
                        result.Append(words[rand.Next(words.Length)]);
                    }
                    result.Append(". ");
                }
                result.Append("</p>");
            }

            return result.ToString();
        }

    }

}
