
#ifndef UDPWRITER_H
#define UDPWRITER_H

#include <iostream>
#include <cstring>

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

class UDPWriter {
    char buffer[256];
    SOCKET sendSocket;

    addrinfo* sendP;

    fd_set writefds;

    sockaddr address{AF_INET, "127.0.0.1"};

public:
    UDPWriter();

    template <typename T>
    void MulticastMessage(MessageType messageType, T message) {
        std::memcpy(buffer, &messageType, sizeof(MessageType));
        std::memcpy(buffer + sizeof(MessageType), &message, sizeof(T));

        FD_ZERO(&writefds);
        FD_SET(sendSocket, &writefds);

        select(sendSocket + 1, nullptr, &writefds, nullptr, nullptr);

        if (FD_ISSET(sendSocket, &writefds)) {
            int result = sendto(sendSocket, buffer, sizeof(MessageType) + sizeof(T), 0, sendP->ai_addr, sendP->ai_addrlen);

            if (result == -1) {
                printf("Error: %d\n", errno);
            } else {
//                printf("Sent!\n");
            }
        } else {
            // Timed out
        }
    }
};

#endif  // UDPWRITER_H
