
#include "Server.h"

#include <iostream>

int main(int argc, char** argv) {
    Server server = Server();

    bool shouldContinue = true;

    std::string input;
    while (shouldContinue) {
        std::cin >> input;
        std::cout << input << std::endl;
        if (input == "quit") {
            shouldContinue = false;
        }
    }

    std::cout << "Returns" << std::endl;

    return 0;
}
