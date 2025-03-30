
#ifndef SERVER_H
#define SERVER_H

#include <future>
#include <map>
#include <vector>

#ifdef _WIN32
#include <ws2tcpip.h>
#include <winsock.h>
#endif

#include "NamedPipeReader.h"
#include "NamedPipeWriter.h"

#include "UDPReader.h"
#include "UDPWriter.h"

#include "Player.h"

class Server {
private:
#ifdef _WIN32
    WSAData wsaData{};
    int result = WSAStartup(MAKEWORD(2, 2), &wsaData);
#endif

    std::map<uint64_t, uint64_t> clientIdMap{};

    // Named pipes
    NamedPipeReader namedPipeReader = NamedPipeReader();
    NamedPipeWriter namedPipeWriter = NamedPipeWriter();

    std::future<void> receiveUpdateAsyncTask;
    std::future<void> receiveClientUpdateAsyncTask;

    // UDP
    UDPReader udpReader = UDPReader();
    UDPWriter udpWriter = UDPWriter();

    // Communication
    [[noreturn]] void ReceiveClientData();
    [[noreturn]] void ReceiveServerData();

   public:
    Server();
    ~Server();
};

#endif  // SERVER_H
