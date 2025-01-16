#ifndef SERVER_H
#define SERVER_H

#include <winsock.h>
#include <ws2tcpip.h>

#include "Game.h"

enum MessageType {
    CONNECT,
    DISCONNECT,
    EVENT,
    MOVE,
    SYNC
};

class Server {
private:
    bool gameRunning = false;
    const unsigned short updateInterval = 50; // milliseconds

    SOCKET listenSocket;
    struct sockaddr_in listenAddr;
    SOCKET sendSocket;
    struct sockaddr_in sendAddr;
    addrinfo* sendP;

    int SendUpdates();

    int ReceiveData();

public:
    Server();

    ~Server();

    Game game;

    int GameLoop();

    int Start();
};

#endif  // SERVER_H
