
#ifndef NAMEDPIPEWRITER_H
#define NAMEDPIPEWRITER_H

#ifdef __linux__
#include <arpa/inet.h>
#include <unistd.h>
#elif _WIN32
#include <winsock2.h>
#include <windows.h>
#include <ws2tcpip.h>
#endif

#include <iostream>
#include <string>

#include "Assets/MessageType.h"

class NamedPipeWriter {
#ifdef __linux__
    int writePipe;
    ssize_t result = 0;
#elif _WIN32
    HANDLE writePipe;
    int result = 0;
    DWORD dwWritten = 0;
#endif

   public:
    NamedPipeWriter();
    virtual ~NamedPipeWriter();

    template <typename T>
    void Write(const T& message) {
#ifdef __linux__
        result = write(writePipe, &message, sizeof(T));
#elif _WIN32
        result = WriteFile(writePipe, &message, sizeof(T), &dwWritten, nullptr);
#endif
    }

    void WriteString(const std::u16string& message) {
        uint16_t lengthNetwork = htons((uint16_t)message.length());

#ifdef __linux__
        result = write(writePipe, &lengthNetwork, sizeof(uint16_t));

        result = write(writePipe, message.c_str(),
                       message.length() * sizeof(char16_t));
#elif _WIN32
        result = WriteFile(writePipe, &lengthNetwork, sizeof(uint16_t),
                           &dwWritten, nullptr);

        result =
            WriteFile(writePipe, message.c_str(),
                      message.length() * sizeof(char16_t), &dwWritten, nullptr);
#endif
    }
};

#endif  // NAMEDPIPEWRITER_H
