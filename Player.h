#ifndef PLAYER_H
#define PLAYER_H

#include <cstdint>
#include <string>

struct Player {
    std::string ip = "";
    uint64_t clientId = 0;
};

#endif  // PLAYER_H
