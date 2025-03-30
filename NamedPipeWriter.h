#ifndef NAMEDPIPEWRITER_H
#define NAMEDPIPEWRITER_H

#ifdef __linux__
typedef int HANDLE;
typedef unsigned int WINBOOL;
#elif _WIN32
#include <ws2tcpip.h>
#include <winsock.h>
#endif
#include <unistd.h>

#include "MessageType.h"

class NamedPipeWriter {
    HANDLE hPipe;
    WINBOOL result = 0;

#ifdef _WIN32
    DWORD dwWritten = 0;
#endif

   public:
    NamedPipeWriter();
    virtual ~NamedPipeWriter();

    template <typename T>
    void WriteMessage(const uint64_t packageId, const MessageType messageType, const T &message) {
#ifdef __linux__
        result = write(hPipe, &packageId, sizeof(uint64_t));

        result = write(hPipe, &messageType, sizeof(MessageType));

        result = write(hPipe, &message, sizeof(T));
#elif _WIN32
        result = WriteFile(hPipe, &packageId, sizeof(uint64_t ), &dwWritten, nullptr);

        result = WriteFile(hPipe, &messageType, sizeof(MessageType), &dwWritten, nullptr);

        result = WriteFile(hPipe, &message, sizeof(T), &dwWritten, nullptr);
#endif
    }
};

#endif  // NAMEDPIPEWRITER_H
