
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
#include <winsock2.h>
#include <ws2tcpip.h>
#endif

#include "macros.h"


#include "MessageType.h"

#define CLIENT_PORT 5000

class UDPWriter {
    char buffer[256];

    fd_set writefds;

    fd_set currentwritefds;
    SOCKET socket;
    std::unordered_map<uint64_t, addrinfo*> addrinfoMap{};
    std::unordered_map<uint64_t, uint64_t> packageIdMap{};
    std::unordered_map<std::string, std::unordered_map<std::string, uint64_t>> clientMap{};

public:
    UDPWriter();

    void SetSocket(SOCKET socket);

    void AddClient(const std::string& ip, const std::string& port, uint64_t clientId);

    void RemoveClient(const std::string& ip, const std::string& port, uint64_t clientId);

    template <typename T>
    void SendUdpMessage(uint64_t clientId, MessageType messageType, T message) {
        printf("SendUdpMessage...");
        if (addrinfoMap.empty()) {
            return;
        }

        std::memcpy(buffer, &(++packageIdMap[clientId]), sizeof(uint64_t));
        std::memcpy(buffer + sizeof(uint64_t), &messageType, sizeof(MessageType));
        std::memcpy(buffer + sizeof(uint64_t) + sizeof(MessageType), &message, sizeof(T));

        FD_ZERO(&writefds);
        FD_SET(socket, &writefds);

        select(socket + 1, nullptr, &writefds, nullptr, nullptr);

        if (FD_ISSET(socket, &writefds)) {
            int result = sendto(socket, buffer, sizeof(uint64_t) + sizeof(MessageType) + sizeof(T), 0, addrinfoMap[clientId]->ai_addr, addrinfoMap[clientId]->ai_addrlen);

            if (result == -1) {
                printf("SendUdpMessage Error: %d\n", errno);
            } else {
                //                printf("Sent!\n");
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

        std::memcpy(buffer + sizeof(uint64_t), &messageType, sizeof(MessageType));
        std::memcpy(buffer + sizeof(uint64_t) + sizeof(MessageType), &message, sizeof(T));

        for (auto addr_info : addrinfoMap) {
            FD_ZERO(&writefds);
            FD_SET(socket, &writefds);
            select(socket + 1, nullptr, &writefds, nullptr, nullptr);

            if (FD_ISSET(socket, &writefds)) {
                std::memcpy(buffer, &(++packageIdMap[addr_info.first]), sizeof(uint64_t));
                int result = sendto(socket, buffer, sizeof(uint64_t) + sizeof(MessageType) + sizeof(T), 0, addr_info.second->ai_addr, addr_info.second->ai_addrlen);

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
