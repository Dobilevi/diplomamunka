
#ifndef UDPREADER_H
#define UDPREADER_H

#include "MessageType.h"

#include <winsock2.h>

class UDPReader {
    SOCKET listenSocket;
    struct sockaddr_in listenAddr;

    fd_set readfds;

    sockaddr address{AF_INET, "127.0.0.1"};
    char buffer[256];

    MessageType messageType;
    uint64_t clientId;
    SpawnMessage spawnMessage;
    UpdateMessage updateMessage;

public:
    UDPReader();

    MessageType ReadMessage();

    MessageType GetMessageType() const;

    uint64_t GetClientId() const;

    UpdateMessage GetUpdateMessage() const;
};

#endif  // UDPREADER_H
