export REDIS_ENDPOINT=darren-demo.lm5w0w.clustercfg.usw2.cache.amazonaws.com 
export REDIS_ENDPOINT=darren-demo.lm5w0w.clustercfg.usw2.cache.amazonaws.com:6379

redis-cli -h $REDIS_ENDPOINT CLUSTER NODES
redis-cli -h 172.31.45.8 MONITOR
redis-cli -h $REDIS_ENDPOINT CLIENT LIST

redis-cli --version

docker compose up
run -e REDIS_ENDPOINT=darren-demo.lm5w0w.clustercfg.usw2.cache.amazonaws.com:6379
