using Amazon.CDK;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SNS.Subscriptions;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.ElastiCache;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.RDS;

namespace Cdk
{

    public class AuroraDatabaseCluster : Stack
    {
        internal AuroraDatabaseCluster(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var vpcId = "vpc-027db8599a32b83e2";
            var vpc2 = Vpc.FromLookup(this, "ExisitingVPC2", new VpcLookupOptions { VpcId = vpcId });
            var mySgForAurora = new SecurityGroup(this, "AuoraPostgreClusterSecurityGroup", new SecurityGroupProps
            {
                AllowAllOutbound = true,
                Vpc = vpc2
            });
            mySgForAurora.AddIngressRule(Peer.AnyIpv4(), Port.Tcp(5432));

            ServerlessCluster cluster = new ServerlessCluster(this, "AnotherCluster", new ServerlessClusterProps
            {
                Engine = DatabaseClusterEngine.AURORA_POSTGRESQL,
                ParameterGroup = ParameterGroup.FromParameterGroupName(this, "ParameterGroup", "default.aurora-postgresql10"),
                Vpc = vpc2,
                SecurityGroups = new SecurityGroup[] {mySgForAurora},
                Scaling = new ServerlessScalingOptions
                {
                    AutoPause = Duration.Minutes(10),  // default is to pause after 5 minutes of idle time
                    MinCapacity = AuroraCapacityUnit.ACU_8,  // default is 2 Aurora capacity units (ACUs)
                    MaxCapacity = AuroraCapacityUnit.ACU_32
                }
            });

            new CfnOutput(this, "DB_ENDPOINT", new CfnOutputProps
            {
                Value = cluster.ClusterEndpoint.ToString()
            });


        } 
    }
    public class ElastiCacheRedisCluster : Stack
    {
        internal ElastiCacheRedisCluster(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {

            // var vpc1 = Vpc.FromLookup(this, "ExisitingVPC", new VpcLookupOptions{VpcId = "vpc-027db8599a32b83e2"});
            string vpcId;
            vpcId = System.Environment.GetEnvironmentVariable("VPC_ID");
            vpcId = "vpc-027db8599a32b83e2";

            var vpc = Vpc.FromLookup(this, "ExisitingVPC", new VpcLookupOptions { VpcId = vpcId });


            var MySubnetGroup = new CfnSubnetGroup(this, "RedisClusterSubnetGroup", new CfnSubnetGroupProps
            {
                CacheSubnetGroupName = "Private",
                SubnetIds = vpc.SelectSubnets(new SubnetSelection { SubnetType = SubnetType.PRIVATE }).SubnetIds,
                Description = "Private Subnets"
            });

            var mySG = new SecurityGroup(this, "RedisClusterSecurityGroup", new SecurityGroupProps
            {
                AllowAllOutbound = true,
                Vpc = vpc
            });
            mySG.AddIngressRule(Peer.AnyIpv4(), Port.Tcp(6379), "Allow Connections from Redis Client");



            // The code that defines your stack goes here
            CfnReplicationGroup elasticache = new CfnReplicationGroup(this, "MyCache", new CfnReplicationGroupProps
            {
                CacheNodeType = "cache.r6g.large",
                Engine = "redis",
                ReplicasPerNodeGroup = 2,
                NumNodeGroups = 2,
                AutomaticFailoverEnabled = true,
                AutoMinorVersionUpgrade = true,
                ReplicationGroupDescription = "Redis Cluster Mode",
                MultiAzEnabled = true,
                CacheSubnetGroupName = MySubnetGroup.CacheSubnetGroupName,
                SecurityGroupIds = new string[] { mySG.SecurityGroupId },
            });

            elasticache.AddDependsOn(MySubnetGroup);

            new CfnOutput(this, "REDIS_ENDPOINT", new CfnOutputProps
            {
                Value = elasticache.AttrConfigurationEndPointAddress
            });

        }
    }
}
