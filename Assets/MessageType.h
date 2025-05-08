#ifndef MESSAGETYPE_H
#define MESSAGETYPE_H

#include <cstdint>

enum MessageType: uint16_t {
    NONE = 0,
    EXIT,
    CONNECT,
    DISCONNECT,
    SPAWN_PROJECTILE,
    INITIALIZE_PROJECTILE,
    DESPAWN,
    UPDATE,
    SPAWN_ENEMY,
    SPAWN_PLAYER,
    INITIALIZE_ENEMY,
    INITIALIZE_PLAYER,
    RESPAWN_ACK,
    ERROR_SERVER_FULL,
    ERROR_OBJECT_DOES_NOT_EXIST
};

enum Spawnable: uint16_t {
    PLAYER = 0,
    EXISTING_PLAYER,
    ENEMY,
    FIRE,
    ROCKET,
    ENEMY_FIRE
};

struct SpawnProjectileMessage {
    Spawnable spawnable;
    uint64_t clientId;
    uint64_t projectileId;
    float x;
    float y;
    float rotation;
};

struct DespawnMessage {
    Spawnable spawnable;
    uint64_t id;
};

enum UpdateType : uint16_t {
    MOVE = 0,
    TELEPORT,
    RESPAWN
};

struct UpdateMessage {
    UpdateType updateType;
    uint64_t clientId;
    float x;
    float y;
    float rotation;
};

struct SpawnMessage {
    uint64_t clientId;
    float x;
    float y;
    float rotation;
};

#endif  // MESSAGETYPE_H
