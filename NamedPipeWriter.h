#ifndef NAMEDPIPEWRITER_H
#define NAMEDPIPEWRITER_H

#include <ws2tcpip.h>
#include <winsock.h>

#include "MessageType.h"

class NamedPipeWriter {
    HANDLE hPipe;
    DWORD dwWritten;
    WINBOOL result;

   public:
    NamedPipeWriter();
    virtual ~NamedPipeWriter();

//    void WriteMessageType(MessageType messageType);
//    void WriteConnectionMessage(uint64_t clientId);
//    void WriteUpdateMessage(UpdateMessage updateMessage);

    template <typename T>
    void WriteMessage(const MessageType &messageType, const T &message) {
        result = WriteFile(hPipe,
                  &messageType,
                  sizeof(MessageType),
                  &dwWritten,
                  NULL);

        result = WriteFile(hPipe,
                           &message,
                           sizeof(T),
                           &dwWritten,
                           NULL);
    }
};

#endif  // NAMEDPIPEWRITER_H
