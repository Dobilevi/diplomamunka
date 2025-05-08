
#ifndef SERVER_H
#define SERVER_H

#include <future>
#include <map>
#include <vector>

#ifdef _WIN32
//#include <ws2tcpip.h>
//#include <winsock.h>
#endif

#include "NamedPipes/NamedPipeReader.h"
#include "NamedPipes/NamedPipeWriter.h"

#include "UDP/UDPReader.h"
#include "UDP/UDPWriter.h"

#include "Assets/Player.h"

class Server {
private:
#ifdef _WIN32
    WSAData wsaData{};
    int result = WSAStartup(MAKEWORD(2, 2), &wsaData);
#endif
    const std::time_t timeOutLimit = 5; // seconds

    std::mutex lastReceivedMessageMapMutex{};
    std::map<uint64_t, time_t> lastReceivedMessageMap;
    std::mutex connectQueueMutex{};
    std::queue<Player> connectQueue{};

    // Named pipes
    NamedPipeReader namedPipeReader = NamedPipeReader();
    NamedPipeWriter namedPipeWriter = NamedPipeWriter();

    std::mutex namedPipeWriterMutex{};

    std::future<void> checkTimeoutAsyncTask;
    std::future<void> receiveUdpAsyncTask;
    std::future<void> receiveUpdateAsyncTask;
    std::future<void> receiveClientUpdateAsyncTask;

    // UDP
    UDPReader udpReader = UDPReader(namedPipeReader.ReadPort());
    UDPWriter udpWriter = UDPWriter();

    // Communication
    [[noreturn]] void ReceiveUdp();
    [[noreturn]] void ReceiveClientData();
    [[noreturn]] void ReceiveServerData();

    void CheckTimeOut();
    [[noreturn]] void CheckTimeOutLoop();

   public:
    Server();
    ~Server();
};

#endif  // SERVER_H
