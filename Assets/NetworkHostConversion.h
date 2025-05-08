
#ifndef NETWORK_HOST_CONVERSION_H
#define NETWORK_HOST_CONVERSION_H

#include "MessageType.h"

#ifdef __linux__
uint32_t htonf();
uint32_t ntohf();
#endif

#if __BIG_ENDIAN__
# define htonll(x) (x)
# define ntohll(x) (x)
#else
# define htonll(x) (((uint64_t)htonl((x) & 0xFFFFFFFF) << 32) | htonl((x) >> 32))
# define ntohll(x) (((uint64_t)ntohl((x) & 0xFFFFFFFF) << 32) | ntohl((x) >> 32))
#endif

void UpdateMessageHostToNetwork(UpdateMessage& updateMessage);

void UpdateMessageNetworkToHost(UpdateMessage& updateMessage);

void SpawnProjectileMessageHostToNetwork(SpawnProjectileMessage& spawnProjectileMessage);

void SpawnProjectileMessageNetworkToHost(SpawnProjectileMessage& spawnProjectileMessage);

void DespawnMessageHostToNetwork(DespawnMessage& despawnMessage);

void DespawnMessageNetworkToHost(DespawnMessage& despawnMessage);

void SpawnMessageHostToNetwork(SpawnMessage& spawnMessage);

void SpawnMessageNetworkToHost(SpawnMessage& spawnMessage);


#endif  // NETWORK_HOST_CONVERSION_H
