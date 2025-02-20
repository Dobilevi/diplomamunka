#ifndef NAMEDPIPEREADER_H
#define NAMEDPIPEREADER_H

#ifdef __linux__
typedef int HANDLE;
typedef unsigned int WINBOOL;
#elif _WIN32
#include <ws2tcpip.h>
#include <winsock.h>
#endif

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
