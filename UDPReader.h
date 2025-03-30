
#ifndef UDPREADER_H
#define UDPREADER_H

#include "MessageType.h"

#ifdef __linux__
#include <sys/types.h>
#include <sys/socket.h>
#include <netdb.h>
#include <arpa/inet.h>
#include <netinet/in.h>
#elif _WIN32
#include <winsock2.h>
typedef int socklen_t;
#endif

#include <map>
#include <vector>

#include "macros.h"
#include "Player.h"

#define SERVER_PORT 4000

class UDPReader {
    SOCKET listenSocket = 0;
    struct sockaddr_in listenAddr{};

    fd_set readfds{};

    sockaddr address{};
    char buffer[256]{};

    std::string ip;
    std::string port;

    uint64_t packageId{};
    MessageType messageType = MessageType::None;
    uint64_t m_clientId{};
    SpawnMessage spawnMessage{};
    UpdateMessage updateMessage{};

    std::map<std::string, std::map<std::string, std::map<uint64_t, Player>>> connectionMap{};

    std::vector<int> clientList{};

public:
    UDPReader();

    SOCKET GetSocket() const;

    bool IsConnected(const std::string& ip, const std::string& port, uint64_t clientId) const;

    void ReadMessage();

    uint64_t GetPackageId() const;

    MessageType GetMessageType() const;

    const std::string& GetIPAddress() const;

    const std::string& GetPortNumber() const;

    uint64_t GetClientId() const;

    const UpdateMessage& GetUpdateMessage() const;

    const SpawnMessage& GetSpawnMessage() const;
};

#endif  // UDPREADER_H
