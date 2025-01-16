#include "Player.h"

std::string Player::ToString() const {
    return std::to_string(x) + "," + std::to_string(y);
}

void Player::SetPos(const float x, const float y) {
    this->x = x;
    this->y = y;
}

float Player::GetPosX() const {
    return x;
}

float Player::GetPosY() const {
    return y;
}
