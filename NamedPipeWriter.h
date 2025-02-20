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
    WINBOOL result;

#ifdef _WIN32
    DWORD dwWritten;
#endif

   public:
    NamedPipeWriter();
    virtual ~NamedPipeWriter();

//    void WriteMessageType(MessageType messageType);
//    void WriteConnectionMessage(uint64_t clientId);
//    void WriteUpdateMessage(UpdateMessage updateMessage);

    template <typename T>
    void WriteMessage(const MessageType &messageType, const T &message) {
#ifdef __linux__
        result = write(hPipe, &messageType, sizeof(MessageType));

        result = write(hPipe, &message, sizeof(T));
#elif _WIN32
        result = WriteFile(hPipe,
                  &messageType,
                  sizeof(MessageType),
                  &dwWritten,
                  nullptr);

        result = WriteFile(hPipe,
                           &message,
                           sizeof(T),
                           &dwWritten,
                           nullptr);
#endif
    }
};

#endif  // NAMEDPIPEWRITER_H
