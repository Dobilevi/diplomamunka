#include "NamedPipeReader.h"

#include <cstdio>
#include <cstdint>
#include <iostream>

NamedPipeReader::NamedPipeReader() {
    std::cout << "Reader Constructor" << std::endl;
    hPipe = CreateNamedPipe(TEXT("\\\\.\\pipe\\CsharpPipe"),
                            PIPE_ACCESS_DUPLEX,
                            PIPE_TYPE_BYTE | PIPE_READMODE_BYTE | PIPE_WAIT,   // FILE_FLAG_FIRST_PIPE_INSTANCE is not needed but forces CreateNamedPipe(..) to fail if the pipe already exists...
                            1,
                            1024 * 16,
                            1024 * 16,
                            NMPWAIT_USE_DEFAULT_WAIT,
                            NULL);

    while (hPipe != INVALID_HANDLE_VALUE) {
            if (ConnectNamedPipe(hPipe, NULL) != FALSE)  // wait for someone to connect to the pipe
                return;
        {
        }
    }
    std::cout << "Reader Constructor End" << std::endl;
}

NamedPipeReader::~NamedPipeReader() {
    DisconnectNamedPipe(hPipe);
}

MessageType NamedPipeReader::ReadMessageType() {
    uint16_t buffer;
    DWORD dwRead;

    DWORD len = sizeof(buffer);

    if ((result = ReadFile(hPipe, &buffer, len, &dwRead, NULL)) != FALSE) {
        /* do something with data in buffer */
        if ((dwRead != len) || !result) {
            throw std::runtime_error("Pipe is closed!");
        }
    }

    return (MessageType)buffer;
}

uint64_t NamedPipeReader::ReadConnectionMessage() {
    uint64_t buffer;
    DWORD dwRead;

    DWORD len = sizeof(buffer);

    if ((result = ReadFile(hPipe, &buffer, len, &dwRead, NULL)) != FALSE) {
        /* do something with data in buffer */
        if ((dwRead != len) || !result) {
            throw std::runtime_error("Pipe is closed!");
        }
    }

    return buffer;
}

UpdateMessage NamedPipeReader::ReadUpdateMessage() {
    UpdateMessage buffer;
    DWORD dwRead;

    DWORD len = sizeof(buffer);

    if ((result = ReadFile(hPipe, &buffer, len, &dwRead, NULL)) != FALSE) {
        /* do something with data in buffer */
        if ((dwRead != len) || !result) {
            throw std::runtime_error("Pipe is closed!");
        }
    }

    return buffer;
}

SpawnMessage NamedPipeReader::ReadSpawnMessage() {
    SpawnMessage buffer;
    DWORD dwRead;

    DWORD len = sizeof(buffer);

    if ((result = ReadFile(hPipe, &buffer, len, &dwRead, NULL)) != FALSE) {
        /* do something with data in buffer */
        if ((dwRead != len) || !result) {
            throw std::runtime_error("Pipe is closed!");
        }
    }

    return buffer;
}
