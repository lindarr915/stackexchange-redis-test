REDIS_ENDPOINT=darren-demo.lm5w0w.clustercfg.usw2.cache.amazonaws.com 

redis-cli -h $REDIS_ENDPOINT MONITOR
redis-cli -h $REDIS_ENDPOINT CLIENT LIST