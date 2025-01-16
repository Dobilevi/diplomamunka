#include "Server.h"

int main(int argc, char** argv) {
    Server server = Server();

    bool shouldContinue = true;
    while (shouldContinue) {
        shouldContinue = server.Start();
    }

    return 0;
}
