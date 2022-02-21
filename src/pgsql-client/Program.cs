using System;
using System.Linq;
using pgsql_client.Models;
using StackExchange.Redis;
using Newtonsoft.Json;
using OpenTelemetry.Trace;
using OpenTelemetry;

namespace EFGetStarted
{
    public class RedisConnectorHelper
    {
        static private string? cacheConnection = System.Environment.GetEnvironmentVariable("REDIS_ENDPOINT") ?? "darren-demo.lm5w0w.clustercfg.usw2.cache.amazonaws.com:6379";
 

        static RedisConnectorHelper()
        {
            RedisConnectorHelper._connection = new Lazy<ConnectionMultiplexer>(() =>
            {
                var connection = ConnectionMultiplexer.Connect(cacheConnection);
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

    internal class Program
    {

        static private TracerProvider tracerProvider = Sdk.CreateTracerProviderBuilder()
                    .AddXRayTraceId()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddOtlpExporter()
                    .AddAWSInstrumentation()
                    .AddRedisInstrumentation(RedisConnectorHelper.Connection,options => options.SetVerboseDatabaseStatements = true)
                    .Build();

        private static void Main()
        {
            IDatabase cache = RedisConnectorHelper.Connection.GetDatabase();

            using (var db = new postgresContext())
            {
                // Note: This sample requires the database to be created before running.

                // Create
                Console.WriteLine("Inserting a new Product");
                db.Add(new Product
                {
                    ProductName = "iPhone 13",
                    Description = "Your new superpower.",
                    Price = 500,
                    QuantityInStock = 100
                });
                db.SaveChanges();

                // Read
                Console.WriteLine("Querying for a Product");
                var Product = db.Products
                    .OrderBy(b => b.ProductId)
                    .First();

                cache.StringSet("Product:1",JsonConvert.SerializeObject(Product));
            }
            using (var db = new postgresContext()){

                var ProductFromCache = JsonConvert.DeserializeObject<Product>(cache.StringGet("Product:1"));

                Console.WriteLine(JsonConvert.SerializeObject(ProductFromCache));

                // Update
                Console.WriteLine("Updating the Product");
                ProductFromCache.Price = 2000;
                db.Update(ProductFromCache);
                db.SaveChanges();

                // Delete
                // Console.WriteLine("Delete the Product");
                // db.Remove(Product);
                // db.SaveChanges();
            }
        }
    }
}