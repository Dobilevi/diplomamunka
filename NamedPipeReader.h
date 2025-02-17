#ifndef NAMEDPIPEREADER_H
#define NAMEDPIPEREADER_H

#include <ws2tcpip.h>
#include <winsock.h>

#include "MessageType.h"


class NamedPipeReader {
    HANDLE hPipe;
    WINBOOL result;

public:
    NamedPipeReader();
    virtual ~NamedPipeReader();

    MessageType ReadMessageType();
    uint64_t ReadConnectionMessage();
    UpdateMessage ReadUpdateMessage();
    SpawnMessage ReadSpawnMessage();
};

#endif  // NAMEDPIPEREADER_H
