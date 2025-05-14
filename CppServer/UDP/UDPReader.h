
#ifndef UDPREADER_H
#define UDPREADER_H

#include "MessageType.h"

#ifdef __linux__
#include <arpa/inet.h>
#elif _WIN32
#include <ws2tcpip.h>
typedef int socklen_t;
#endif

#include <map>
#include <mutex>
#include <queue>
#include <vector>

#include "Buffer.h"
#include "Player.h"

class UDPReader {
#ifdef __linux__
    int listenSocket = 0;
#elif _WIN32
    SOCKET listenSocket = 0;
#endif
    fd_set readfds{};

    sockaddr address{};
    Buffer buffer = Buffer(256);  // TODO: size?

    std::string ip;
    std::string port;

    // Raw value storage
    uint64_t packageIdNetwork{};
    uint16_t messageTypeNetwork = 0;
    uint16_t errorTypeNetwork = 0;
    uint64_t m_clientIdNetwork{};
    uint16_t lengthNetwork = 0;
    std::u16string playerNameNetwork{};
    SpawnProjectileMessage spawnMessageNetwork{};
    UpdateMessage updateMessageNetwork{};

    // Converted value storage
    uint64_t packageIdHost{};
    MessageType messageTypeHost = MessageType::NONE;
    uint16_t lengthHost = 0;
    uint64_t m_clientIdHost = 0;

    std::mutex clientMutex{};
    std::map<uint64_t, std::pair<std::string, std::string>> clientAddressMap{};
    std::map<std::string, std::map<std::string, uint64_t>> connectionMap{};
    std::map<std::string, std::map<std::string, bool>> waitingMap{};
    std::map<std::string, std::map<std::string, uint64_t>> packageIdMap{};

    struct UDP {
        uint16_t size;
        char* buffer;
        std::string ip;
        std::string port;
    };
    std::queue<UDP> udpQueue{};
    std::mutex udpQueueMutex{};

    static const uint16_t maxPlayerNameLength = 32;

   public:
#ifdef __linux__
    void SetSocket(int newSocket);
#elif _WIN32
    void SetSocket(SOCKET newSocket);
#endif

    void AddClient(const std::string& ip, const std::string& port,
                   uint64_t clientId);
    void RemoveClient(uint64_t clientId);

    bool IsConnected(const std::string& ip, const std::string& port);
    bool IsConnected(const std::string& ip, const std::string& port,
                     uint64_t clientId);

    void CheckPackageId();

    void ReadUdpPackage();
    void ReadMessage();
    void ProcessMessage();

    const std::string& GetIPAddress() const;
    const std::string& GetPortNumber() const;
    uint64_t GetSenderClientId() const;

    uint64_t GetPackageId() const;
    uint16_t GetMessageType() const;
    MessageType GetMessageTypeHost() const;
    uint16_t GetErrorType() const;
    uint64_t GetClientId() const;
    uint64_t GetClientIdHost() const;
    const std::u16string& GetPlayerName() const;
    const UpdateMessage& GetUpdateMessage() const;
    const SpawnProjectileMessage& GetSpawnMessage() const;
};

#endif  // UDPREADER_H
