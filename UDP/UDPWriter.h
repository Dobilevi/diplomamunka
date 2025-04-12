
#ifndef UDPWRITER_H
#define UDPWRITER_H

#include <cstring>
#include <iostream>
#include <list>
#include <unordered_map>

#ifdef __linux__
#include <sys/types.h>
#include <sys/socket.h>
#include <netdb.h>
#include <arpa/inet.h>
#include <netinet/in.h>
#elif _WIN32
#include <ws2tcpip.h>
#endif

#include "Buffer.h"
#include "MessageType.h"
#include "NetworkHostConversion.h"

class UDPWriter {
    Buffer buffer = Buffer(256);

    fd_set writefds;
    fd_set currentwritefds;
#ifdef __linux__
    int socket;
#elif _WIN32
    SOCKET socket;
#endif

    std::unordered_map<uint64_t, addrinfo*> addrinfoMap{};
    std::unordered_map<uint64_t, uint64_t> packageIdMap{};

public:
    UDPWriter();

#ifdef __linux__
    void SetSocket(int socket);
#elif _WIN32
    void SetSocket(SOCKET socket);
#endif

    void AddClient(const std::string& ip, const std::string& port, uint64_t clientId);

    void RemoveClient(uint64_t clientId);

    template <typename T>
    void SendUdpMessage(uint64_t clientId, MessageType messageType, T message) {
        if (addrinfoMap.empty()) {
            return;
        }

        buffer.Reset();

        buffer.Write(htonll(++packageIdMap[clientId]));
        buffer.Write(messageType);
        buffer.Write(message);

        FD_ZERO(&writefds);
        FD_SET(socket, &writefds);

        select(socket + 1, nullptr, &writefds, nullptr, nullptr);

        if (FD_ISSET(socket, &writefds)) {
            int result = sendto(socket, buffer.GetBuffer(), buffer.GetSize(), 0, addrinfoMap[clientId]->ai_addr, addrinfoMap[clientId]->ai_addrlen);

            if (result == -1) {
                printf("SendUdpMessage Error: %d\n", errno);
            }
        } else {
            // Timed out
            printf("SendUdpMessage TIMEOUT: socket: %llu!\n", socket);
        }
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

        FD_ZERO(&writefds);
        FD_SET(socket, &writefds);

        select(socket + 1, nullptr, &writefds, nullptr, nullptr);

        if (FD_ISSET(socket, &writefds)) {
            int result = sendto(socket, buffer.GetBuffer(), buffer.GetSize(), 0, addrinfoMap[clientId]->ai_addr, addrinfoMap[clientId]->ai_addrlen);

            if (result == -1) {
                printf("SendUdpMessage Error: %d\n", errno);
            }
        } else {
            // Timed out
            printf("SendUdpMessage TIMEOUT: socket: %llu!\n", socket);
        }
    }

    template <typename T>
    void MulticastMessage(MessageType messageType, T message) {
        if (addrinfoMap.empty()) {
            return;
        }

        buffer.Reset(sizeof(uint64_t)); // Space for packageId

        buffer.Write(messageType);
        buffer.Write(message);

        uint64_t packageId;
        for (auto addr_info : addrinfoMap) {
            FD_ZERO(&writefds);
            FD_SET(socket, &writefds);
            select(socket + 1, nullptr, &writefds, nullptr, nullptr);

            if (FD_ISSET(socket, &writefds)) {
                packageId = htonll(++packageIdMap[addr_info.first]);
                std::memcpy(buffer.GetBuffer(), &packageId, sizeof(uint64_t));
                int result = sendto(socket, buffer.GetBuffer(), buffer.GetSize(), 0, addr_info.second->ai_addr, addr_info.second->ai_addrlen);

                if (result == -1) {
                    printf("Error: %d, num of clients: %d, socket: %llu\n", errno, addrinfoMap.size(), socket);
                } else {
//                printf("Sent!\n");
                }
            } else {
                // Timed out
                printf("TIMEOUT: socket: %d!\n", socket);
            }
        }
    }

    template <typename T>
    void MulticastSpawnPlayerMessage(MessageType messageType, T message, const std::u16string& playerName) {
        if (addrinfoMap.empty()) {
            return;
        }

        buffer.Reset(sizeof(uint64_t)); // Space for packageId

        buffer.Write(messageType);
        buffer.Write(message);
        buffer.Write(htons((uint16_t)playerName.length()));
        buffer.WriteString(playerName);

        for (auto addr_info : addrinfoMap) {
            FD_ZERO(&writefds);
            FD_SET(socket, &writefds);
            select(socket + 1, nullptr, &writefds, nullptr, nullptr);

            if (FD_ISSET(socket, &writefds)) {
                uint64_t packageId = htonll(++packageIdMap[addr_info.first]);
                std::memcpy(buffer.GetBuffer(), &packageId, sizeof(uint64_t));
                int result = sendto(socket, buffer.GetBuffer(), buffer.GetSize(), 0, addr_info.second->ai_addr, addr_info.second->ai_addrlen);

                if (result == -1) {
                    printf("Error: %d, num of clients: %d, socket: %llu\n", errno, addrinfoMap.size(), socket);
                } else {
                    //                printf("Sent!\n");
                }
            } else {
                // Timed out
                printf("TIMEOUT: socket: %d!\n", socket);
            }
        }
    }
};

#endif  // UDPWRITER_H
