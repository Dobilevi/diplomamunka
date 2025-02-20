
#include "UDPReader.h"

#include <cassert>
#include <cstring>

#include <iostream>

UDPReader::UDPReader() {
    listenSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);

    assert(listenSocket != INVALID_SOCKET);

    listenAddr.sin_addr.s_addr = inet_addr("127.0.0.1");
    listenAddr.sin_family = AF_INET;
    listenAddr.sin_port = htons(4000);

    int result = bind(listenSocket, (sockaddr*)&listenAddr, sizeof(listenAddr));
    assert(result != SOCKET_ERROR);

//    setsockopt(listenSocket, SOL_SOCKET, SO_RCVBUF, "65536", 5);
}

MessageType UDPReader::ReadMessage() {
    FD_ZERO(&readfds);
    FD_SET(listenSocket, &readfds);

    socklen_t fromlen = sizeof(address);

    int res = select(listenSocket + 1, &readfds, nullptr, nullptr, nullptr);
//    printf("Select: %d\n", res);
    if (FD_ISSET(listenSocket, &readfds)) {
        int received = recvfrom(listenSocket, buffer, 256, 0, &address, &fromlen);
//        printf("Error: %d\n", WSAGetLastError());

        std::memcpy(&messageType, buffer, sizeof(MessageType));
//        printf("Read size: %d, type: %u\n", received, messageType);

        switch (messageType) {
            case Connect:
            case Disconnect:
                std::memcpy(&clientId, buffer + sizeof(MessageType), sizeof(SpawnMessage));
                break;
            case Spawn:
                std::memcpy(&spawnMessage, buffer + sizeof(MessageType), sizeof(SpawnMessage));
                break;
            case Update:
                std::memcpy(&updateMessage, buffer + sizeof(MessageType), sizeof(UpdateMessage));
                break;
            default:
                break;
        }

    } else {
        // Timeout
    }

    return messageType;
}

MessageType UDPReader::GetMessageType() const {
    return messageType;
}

uint64_t UDPReader::GetClientId() const {
    return clientId;
}

UpdateMessage UDPReader::GetUpdateMessage() const {
    return updateMessage;
}
