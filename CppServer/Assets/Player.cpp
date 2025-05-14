#include "Player.h"

Player::Player(const std::string& ip, const std::string& port,
               uint64_t clientId, const std::u16string& nickname)
    : ip(ip), port(port), clientId(clientId), nickname(nickname) {}
