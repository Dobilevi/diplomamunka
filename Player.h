#ifndef PLAYER_H
#define PLAYER_H

#include <string>

class Player {
private:
    constexpr static const float speed = 10.0f;

    unsigned short int id = 0;

    float x = 0.0f;
    float y = 0.0f;

   public:
    std::string ToString() const;

    void SetPos(float x, float y);

    float GetPosX() const;

    float GetPosY() const;
};

#endif  // PLAYER_H
