using Amazon.CDK;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SNS.Subscriptions;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.ElastiCache;
using Amazon.CDK.AWS.EC2;

namespace Cdk
{
    public class ElastiCacheRedisCluster : Stack
    {
        internal ElastiCacheRedisCluster(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {

            // var vpc1 = Vpc.FromLookup(this, "ExisitingVPC", new VpcLookupOptions{VpcId = "vpc-027db8599a32b83e2"});
            string vpcId;
                vpcId = System.Environment.GetEnvironmentVariable("VPC_ID");
                vpcId = "vpc-027db8599a32b83e2";

            var vpc2 = Vpc.FromLookup(this, "ExisitingVPC", new VpcLookupOptions{VpcId = vpcId});

            var MySubnetGroup = new CfnSubnetGroup(this, "RedisClusterSubnetGroup", new CfnSubnetGroupProps{
                CacheSubnetGroupName = "Private",
                SubnetIds = vpc2.SelectSubnets(new SubnetSelection{SubnetType = SubnetType.PRIVATE}).SubnetIds,
                Description = "Private Subnets"
            });

            var mySG = new SecurityGroup(this, "RedisClusterSecurityGroup",new SecurityGroupProps{
                AllowAllOutbound = true,
                Vpc = vpc2
            });
            mySG.AddIngressRule(Peer.AnyIpv4(),Port.Tcp(6379),"Allow Connections from Redis Client");

            // The code that defines your stack goes here
            CfnReplicationGroup elasticache = new CfnReplicationGroup(this, "MyCache", new CfnReplicationGroupProps
            {
                CacheNodeType = "cache.r6g.large",
                Engine = "redis",
                ReplicasPerNodeGroup = 2,
                NumNodeGroups = 4,
                AutomaticFailoverEnabled = true,
                AutoMinorVersionUpgrade = true,
                ReplicationGroupDescription = "Redis Cluster Mode",
                MultiAzEnabled = true,
                CacheSubnetGroupName = MySubnetGroup.CacheSubnetGroupName,
                SecurityGroupIds = new string[] {mySG.SecurityGroupId},
            });

            elasticache.AddDependsOn(MySubnetGroup);

            new CfnOutput(this, "REDIS_ENDPOINT",new CfnOutputProps{
                Value = elasticache.AttrConfigurationEndPointAddress
            });

        }
    }
}
