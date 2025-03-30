#include "Player.h"

uint64_t Player::nextClientId = 0;
std::set<uint64_t> Player::clientIdSet = std::set<uint64_t>();

uint64_t Player::GetNextClientId() {
    uint64_t clientId;

    while (true) {
        clientId = nextClientId++;

        if (clientIdSet.find(clientId) == clientIdSet.end()) {
            return clientId;
        }
    }
}

Player::Player(const std::string& ip, const std::string& port, uint64_t clientId, std::string&& nickname)
 : ip(ip), port(port), clientId(clientId), nickname(nickname) {}
