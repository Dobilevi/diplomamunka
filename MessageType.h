#ifndef MESSAGETYPE_H
#define MESSAGETYPE_H

#include <cstdint>

enum MessageType: uint16_t
{
    None,
    Exit,
    Connect,
    Disconnect,
    Spawn,
    Update,
    SetStartPosition
};

enum Spawnable: uint16_t {
    FIRE,
    ROCKET
};

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
};

#endif  // MESSAGETYPE_H
