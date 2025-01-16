#ifndef GAME_H
#define GAME_H

#include "Player.h"

#include <vector>

class Game {
   private:
    static const unsigned int maxPlayerCount = 5;

    void StartServer() {

    }

   public:
    std::vector<Player> players;
    unsigned int playerCount = 0;

    Game();
};

#endif  // GAME_H
