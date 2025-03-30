#ifndef PLAYER_H
#define PLAYER_H

#include <cstdint>
#include <set>
#include <string>

#include <inaddr.h>

struct Player {
    std::string ip;
    std::string port;
    uint64_t clientId = 0;
    std::string nickname = "Player";

    static uint64_t nextClientId;
    static std::set<uint64_t> clientIdSet;

    Player() = default;
    Player(const std::string& ip, const std::string& port, uint64_t clientId, std::string&& nickname);

    static uint64_t GetNextClientId();
};

#endif  // PLAYER_H
