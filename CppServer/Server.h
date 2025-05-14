
#ifndef SERVER_H
#define SERVER_H

#include <future>
#include <map>
#include <vector>

#include "Assets/Player.h"
#include "NamedPipes/NamedPipeReader.h"
#include "NamedPipes/NamedPipeWriter.h"
#include "UDP/UDPReader.h"
#include "UDP/UDPWriter.h"

class Server {
   private:
#ifdef __linux__
    int serverSocket = 0;
    int result;
#elif _WIN32
    SOCKET serverSocket = 0;
    WSAData wsaData{};
    int result = WSAStartup(MAKEWORD(2, 2), &wsaData);
#endif
    const std::time_t timeOutLimit = 5;  // seconds

    std::mutex lastReceivedMessageMapMutex{};
    std::map<uint64_t, time_t> lastReceivedMessageMap;

    std::mutex connectQueueMutex{};
    std::queue<Player> connectQueue{};

    // Named pipes
    std::mutex namedPipeWriterMutex{};
    NamedPipeWriter namedPipeWriter = NamedPipeWriter();
    NamedPipeReader namedPipeReader = NamedPipeReader();

    // Async tasks
    std::future<void> checkTimeoutAsyncTask;
    std::future<void> receiveUdpAsyncTask;
    std::future<void> receiveUpdateAsyncTask;
    std::future<void> receiveClientUpdateAsyncTask;

    // UDP
    UDPWriter udpWriter = UDPWriter();
    UDPReader udpReader = UDPReader();

    // Communication
    [[noreturn]] void ReceiveUDPMessages();
    [[noreturn]] void ReceiveClientData();
    [[noreturn]] void ReceiveServerData();

    void CheckTimeOut();
    [[noreturn]] void CheckTimeOutLoop();

   public:
    Server();
    ~Server();
};

#endif  // SERVER_H
