
#ifndef NAMEDPIPEWRITER_H
#define NAMEDPIPEWRITER_H

#ifdef __linux__
typedef int HANDLE;
typedef unsigned int WINBOOL;
#elif _WIN32
#include <ws2tcpip.h>
#endif
#include <unistd.h>

#include <iostream>
#include <string>

#include "Assets/MessageType.h"

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
    void Write(const T &message) {
#ifdef __linux__
        result = write(hPipe, &message, sizeof(T));
#elif _WIN32
        result = WriteFile(hPipe, &message, sizeof(T), &dwWritten, nullptr);
#endif
    }

    void WriteString(const std::u16string& message) {
        uint16_t lengthNetwork = htons((uint16_t)message.length());

#ifdef __linux__
        result = write(hPipe, &lengthNetwork, sizeof(uint16_t));

        result = write(hPipe, message.c_str(), message.length() * sizeof(char16_t));
#elif _WIN32
        result = WriteFile(hPipe, &lengthNetwork, sizeof(uint16_t), &dwWritten, nullptr);

        result = WriteFile(hPipe, message.c_str(), message.length() * sizeof(char16_t), &dwWritten, nullptr);
#endif
    }

    void WriteConnectMessage(const MessageType messageType, const uint16_t length, const std::u16string& message) {
#ifdef __linux__
        result = write(hPipe, &messageType, sizeof(MessageType));

//        result = write(hPipe, &clientId, sizeof(uint64_t));

        result = write(hPipe, &length, sizeof(uint16_t));

        result = write(hPipe, message.c_str(), length * sizeof(char16_t));
#elif _WIN32
        result = WriteFile(hPipe, &messageType, sizeof(MessageType), &dwWritten, nullptr);

        result = WriteFile(hPipe, &length, sizeof(uint16_t), &dwWritten, nullptr);

        result = WriteFile(hPipe, message.c_str(), length * sizeof(char16_t), &dwWritten, nullptr);
#endif
    }
};

#endif  // NAMEDPIPEWRITER_H
