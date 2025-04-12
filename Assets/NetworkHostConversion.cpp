
#include "NetworkHostConversion.h"

//#include <arpa/inet.h>
//#include <netinet/in.h>

#include <cstring>
#include <string>
#include <algorithm>

#include <ws2tcpip.h>
#include <winsock2.h>

uint32_t htonf(float hostfloat) {
    uint32_t buffer;
    std::memcpy(&buffer, &hostfloat, sizeof(float));

    return htonl(buffer);
}

float ntohf(uint32_t netfloat) {
    netfloat = ntohl(netfloat);

    float buffer;
    std::memcpy(&buffer, &netfloat, sizeof(float));

    return buffer;
}

void StringHostToNetwork(std::u16string& val) {
    std::for_each(val.begin(), val.end(), htons);
}

void StringNetworkToHost(std::u16string& val) {
    std::for_each(val.begin(), val.end(), ntohs);
}

void UpdateMessageHostToNetwork(UpdateMessage& updateMessage) {
    updateMessage.updateType = (UpdateType)htons(updateMessage.updateType);
    updateMessage.clientId = htonll(updateMessage.clientId);
    updateMessage.x = htonf(updateMessage.x);
    updateMessage.y = htonf(updateMessage.x);
    updateMessage.rotation = htonf(updateMessage.x);
}

void UpdateMessageNetworkToHost(UpdateMessage& updateMessage) {
    updateMessage.updateType = (UpdateType)ntohs(updateMessage.updateType);
    updateMessage.clientId = ntohll(updateMessage.clientId);
    updateMessage.x = ntohf(updateMessage.x);
    updateMessage.y = ntohf(updateMessage.x);
    updateMessage.rotation = ntohf(updateMessage.x);
}

void SpawnProjectileMessageHostToNetwork(SpawnProjectileMessage& spawnProjectileMessage) {
    spawnProjectileMessage.spawnable = (Spawnable)htons(spawnProjectileMessage.spawnable);
    spawnProjectileMessage.clientId = htonll(spawnProjectileMessage.clientId);
    spawnProjectileMessage.projectileId = htonll(spawnProjectileMessage.projectileId);
    spawnProjectileMessage.x = htonf(spawnProjectileMessage.x);
    spawnProjectileMessage.y = htonf(spawnProjectileMessage.x);
    spawnProjectileMessage.rotation = htonf(spawnProjectileMessage.x);
}

void SpawnProjectileMessageNetworkToHost(SpawnProjectileMessage& spawnProjectileMessage) {
    spawnProjectileMessage.spawnable = (Spawnable)ntohs(spawnProjectileMessage.spawnable);
    spawnProjectileMessage.clientId = ntohll(spawnProjectileMessage.clientId);
    spawnProjectileMessage.projectileId = ntohll(spawnProjectileMessage.projectileId);
    spawnProjectileMessage.x = ntohf(spawnProjectileMessage.x);
    spawnProjectileMessage.y = ntohf(spawnProjectileMessage.x);
    spawnProjectileMessage.rotation = ntohf(spawnProjectileMessage.x);
}

void DespawnMessageHostToNetwork(DespawnMessage& despawnMessage) {
    despawnMessage.spawnable = (Spawnable)htons(despawnMessage.spawnable);
    despawnMessage.id = htonll(despawnMessage.id);
}

void DespawnMessageNetworkToHost(DespawnMessage& despawnMessage) {
    despawnMessage.spawnable = (Spawnable)ntohs(despawnMessage.spawnable);
    despawnMessage.id = ntohll(despawnMessage.id);
}

void SpawnMessageHostToNetwork(SpawnMessage& spawnMessage) {
    spawnMessage.spawnable = (Spawnable)htons(spawnMessage.spawnable);
    spawnMessage.clientId = htonll(spawnMessage.clientId);
    spawnMessage.x = htonf(spawnMessage.x);
    spawnMessage.y = htonf(spawnMessage.x);
    spawnMessage.rotation = htonf(spawnMessage.x);
}

void SpawnMessageNetworkToHost(SpawnMessage& spawnMessage) {
    spawnMessage.clientId = ntohll(spawnMessage.clientId);
    spawnMessage.x = ntohf(spawnMessage.x);
    spawnMessage.y = ntohf(spawnMessage.x);
    spawnMessage.rotation = ntohf(spawnMessage.x);
}
