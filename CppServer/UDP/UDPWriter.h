
#ifndef UDPWRITER_H
#define UDPWRITER_H

#include <cstring>
#include <iostream>
#include <list>
#include <unordered_map>

#ifdef __linux__
#include <netdb.h>
#include <arpa/inet.h>
#elif _WIN32
#include <ws2tcpip.h>
#endif

#include "Buffer.h"
#include "MessageType.h"
#include "NetworkHostConversion.h"

class UDPWriter {
    Buffer buffer = Buffer(256); // TODO: size?

    fd_set writefds;
#ifdef __linux__
    int writeSocket;
#elif _WIN32
    SOCKET writeSocket;
#endif

    std::unordered_map<uint64_t, addrinfo*> addrinfoMap{};
    std::unordered_map<uint64_t, uint64_t> packageIdMap{};

public:
#ifdef __linux__
    void SetSocket(int newSocket);
#elif _WIN32
    void SetSocket(SOCKET newSocket);
#endif

    bool AddClient(const std::string& ip, const std::string& port, uint64_t clientId);
    void RemoveClient(uint64_t clientId);

    void SendUDPPackage(addrinfo* adddr);
    void MulticastUDPPackage();
    void SendErrorMessage(const std::string& ip, const std::string& port, MessageType messageType);

    template <typename T>
    void SendUdpMessage(uint64_t clientId, MessageType messageType, T message) {
        if (addrinfoMap.empty()) {
            return;
        }

        buffer.Reset();

        buffer.Write(htonll(++packageIdMap[clientId]));
        buffer.Write(messageType);
        buffer.Write(message);

        SendUDPPackage(addrinfoMap[clientId]);
    }

    template <typename T>
    void SendSpawnPlayerUdpMessage(uint64_t clientId, MessageType messageType, T message, const std::u16string& playerName) {
        if (addrinfoMap.empty()) {
            return;
        }

        buffer.Reset();

        buffer.Write(htonll(++packageIdMap[clientId]));
        buffer.Write(messageType);
        buffer.Write(message);
        buffer.Write(htons((uint16_t)playerName.length()));
        buffer.WriteString(playerName);

        SendUDPPackage(addrinfoMap[clientId]);
    }

    template <typename T>
    void MulticastMessage(MessageType messageType, T message) {
        if (addrinfoMap.empty()) {
            return;
        }

        buffer.Reset(sizeof(uint64_t)); // Space for packageId

        buffer.Write(messageType);
        buffer.Write(message);

        MulticastUDPPackage();
    }

    template <typename T>
    void MulticastSpawnPlayerMessage(MessageType messageType, T& message, uint16_t length, const std::u16string& playerName) {
        if (addrinfoMap.empty()) {
            return;
        }

        buffer.Reset(sizeof(uint64_t)); // Space for packageId

        buffer.Write(messageType);
        buffer.Write(message);
        buffer.Write(length);
        buffer.WriteString(playerName);

        MulticastUDPPackage();
    }
};

#endif  // UDPWRITER_H
