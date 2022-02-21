using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();

            new ElastiCacheRedisCluster(app, "ElastiCacheRedisCluster", new StackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Account = "091550601287",
                    Region = "us-west-2",
                }
            }
            );

            new AuroraDatabaseCluster(app, "MyAuroraDatabaseCluster", new StackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Account = "091550601287",
                    Region = "us-west-2",
                }
            }
            );
            app.Synth();
        }
    }
}
