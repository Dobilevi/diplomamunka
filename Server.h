
#ifndef SERVER_H
#define SERVER_H

#include <future>
#include <vector>

#include <ws2tcpip.h>
#include <winsock.h>

#include "NamedPipeReader.h"
#include "NamedPipeWriter.h"

#include "UDPReader.h"
#include "UDPWriter.h"

#include "Player.h"

class Server {
private:
    WSAData wsaData;
    int res = WSAStartup(MAKEWORD(2, 2), &wsaData);

    bool gameRunning = false;

    SOCKET listenSocket;
    struct sockaddr_in listenAddr;
    SOCKET sendSocket;
    struct sockaddr_in sendAddr;
    addrinfo* sendP;

    // Named pipes
    NamedPipeReader namedPipeReader;
    NamedPipeWriter namedPipeWriter;

    std::future<void> receiveUpdateAsyncTask;
    std::future<void> receiveClientUpdateAsyncTask;

    // UDP
    UDPReader udpReader;
    UDPWriter udpWriter;

    // Logic
    std::vector<Player> players;

    //
    [[noreturn]] void ReceiveClientData();
    [[noreturn]] void ReceiveServerData();
   public:

    Server();

    ~Server();

    int SendUpdates();

    int Start();

    bool Test();
};

#endif  // SERVER_H
