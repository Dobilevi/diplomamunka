
#ifndef NAMEDPIPEREADER_H
#define NAMEDPIPEREADER_H

#include <cstring>
#include <stdexcept>

#if _WIN32
#include <ws2tcpip.h>
#endif

#include "Assets/MessageType.h"

class NamedPipeReader {
#ifdef __linux__
    int listenPipe;
    ssize_t result = 0;
#elif _WIN32
    HANDLE listenPipe;
    int result = 0;
#endif

   public:
    NamedPipeReader();
    virtual ~NamedPipeReader();

    uint16_t ReadPort();
    void ReadString(uint16_t length, std::u16string& out);

    template <typename T>
    void Read(T& out) {
#ifdef __linux__
        if ((result = read(listenPipe, &out, sizeof(T))) < 0) {
            throw std::runtime_error(std::strerror(errno));
        }
#elif _WIN32
        DWORD dwRead;

        DWORD len = sizeof(T);

        if ((result = ReadFile(listenPipe, &out, len, &dwRead, nullptr)) !=
            FALSE) {
            if ((dwRead != len) || !result) {
                throw std::runtime_error("Pipe is closed!");
            }
        }
#endif
    }
};

#endif  // NAMEDPIPEREADER_H
