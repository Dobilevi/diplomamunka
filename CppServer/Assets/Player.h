#ifndef PLAYER_H
#define PLAYER_H

#include <cstdint>
#include <set>
#include <string>

struct Player {
    std::string ip;
    std::string port;
    uint64_t clientId = 0;
    std::u16string nickname;

    Player() = default;
    Player(const std::string& ip, const std::string& port, uint64_t clientId,
           const std::u16string& nickname);
};

#endif  // PLAYER_H
