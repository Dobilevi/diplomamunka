#include "Server.h"

#include <iostream>

int main(int argc, char** argv) {
    Server server = Server();

    bool shouldContinue = true;

    std::string input;
    while (shouldContinue) {
        std::cin >> input;

        if (input == "quit") {
            shouldContinue = false;
        }
    }

    return 0;
}
