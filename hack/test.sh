export REDIS_ENDPOINT=darren-demo.lm5w0w.clustercfg.usw2.cache.amazonaws.com 
export REDIS_ENDPOINT=darren-demo.lm5w0w.clustercfg.usw2.cache.amazonaws.com:6379


redis-cli -h $REDIS_ENDPOINT CLUSTER NODES
redis-cli -h 172.31.45.8 MONITOR
redis-cli -h $REDIS_ENDPOINT CLIENT LIST

redis-cli --version

docker compose up
run -e REDIS_ENDPOINT=darren-demo.lm5w0w.clustercfg.usw2.cache.amazonaws.com:6379
k -n default exec -it ubuntu-787f7d6d7c-trp9f -- /bin/bash

redis-cli -h $REDIS_ENDPOINT CLUSTER KEYSLOT $KEY
redis-cli -h $REDIS_ENDPOINT CLUSTER GETKEYSINSLOT 15200 3

redis-cli -h 172.31.12.247 GET 
22e611a8-be9d-4589-b1b7-aacf86b4f7ef
redis-cli -h 172.31.40.201 GET 3a94283b-49e8-4481-94e5-b75eeb0caa89
redis-cli -h 192.168.181.211 GET 22e611a8-be9d-4589-b1b7-aacf86b4f7ef
redis-cli -h 192.168.98.89 KEYS

REDIS_ENDPOINT=cdm9m8ue6l9kg5n.lm5w0w.clustercfg.usw2.cache.amazonaws.com
KEY=001ba8a0-df21-40f4-88c0-73a0bcc496d7


redis-cli -h 192.168.97.250 GET $KEY
redis-cli -h 192.168.98.89 GET $KEY

DB_ENDPOINT=database-1.cluster-cyeqog6cufmf.us-west-2.rds.amazonaws.com
psql -h $DB_ENDPOINT -U postgres


k -n default exec -it ubuntu-787f7d6d7c-trp9f -- /bin/bash

f563001ebaf7635839b8589d3096f85f746386ca 172.31.9.246:6379@1122 slave 99548c9ddaf8b0615db74af2e0b2005302e6e963 0 1634888345377 1620 connected
358fe18cbfdd88983651525758d5dbd1235bbba9 172.31.41.207:6379@1122 slave 0007db69c2ad65eaf33e5f7e4bad28d1d2b3df37 0 1634888346380 1619 connected
0007db69c2ad65eaf33e5f7e4bad28d1d2b3df37 172.31.12.247:6379@1122 myself,master - 0 1634888346000 1619 connected 0-3084 3563-3928 4314-5461 5513-5714 6254-6297 8285-8917 9529-9649 9884-10429 11131-11949 12569-13094 13778-14479
99548c9ddaf8b0615db74af2e0b2005302e6e963 172.31.40.201:6379@1122 master - 0 1634888347383 1620 connected 3085-3562 3929-4313 5462-5512 5715-6253 6298-8284 8918-9528 9650-9883 10430-11130 11950-12568 13095-13777 14480-16383

DB_ENDPOINT=database-1.cluster-cyeqog6cufmf.us-west-2.rds.amazonaws.com
DB_PASSWORD=Uudaew2eiyua8thei2wo
REDIS_ENDPOINT=cdm9m8ue6l9kg5n.lm5w0w.clustercfg.usw2.cache.amazonaws.com

psql -h $DB_ENDPOINT -U postgres


redis-cli -h $REDIS_ENDPOINT -p 6379 --csv psubscribe '*'
redis-cli -h $REDIS_ENDPOINT -p 6379 --csv subscribe "__keyevent@0__:set"
'__keyspace@0__:59b47939-d1bb-4f6e-addb-edb8ef790d82'