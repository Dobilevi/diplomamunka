#ifndef MESSAGETYPE_H
#define MESSAGETYPE_H

#include <cstdint>

enum MessageType: uint16_t
{
    Connect,
    Disconnect,
    Spawn,
    Update,
};

struct ConnectionMessage
{
    uint64_t clientId;
};

enum Spawnable {
    PLAYER,
    FIRE,
    ROCKET };

struct SpawnMessage {
   uint64_t clientId;
   Spawnable spawnable;
   float x;
   float y;
   float rotation;
};

struct UpdateMessage
{
    uint64_t clientId;
    float x;
    float y;
    float rotation;
    char isFiring; // bool
};

#endif  // MESSAGETYPE_H
