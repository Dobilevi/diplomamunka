
#ifndef NAMEDPIPEREADER_H
#define NAMEDPIPEREADER_H

#ifdef __linux__
typedef int HANDLE;
typedef unsigned int WINBOOL;
#elif _WIN32
#include <ws2tcpip.h>

#include <stdexcept>
#include <string>
#endif

#include "Assets/MessageType.h"


class NamedPipeReader {
    HANDLE hPipe;
    WINBOOL result = 0;

public:
    NamedPipeReader();
    virtual ~NamedPipeReader();

    uint16_t ReadPort() {
        uint16_t out;

#ifdef __linux__
        if ((result = read(hPipe, &out, sizeof(uint16_t))) < 0) {
            throw std::runtime_error(strerror(errno));
        }
#elif _WIN32
        DWORD dwRead;

        DWORD len = sizeof(uint16_t);

        if ((result = ReadFile(hPipe, &out, len, &dwRead, nullptr)) != FALSE) {
            if ((dwRead != len) || !result) {
                throw std::runtime_error("Pipe is closed!");
            }
        }
#endif

        return ntohs(out);
    }

    template <typename T>
    void Read(T& out) {
#ifdef __linux__
        if ((result = read(hPipe, &out, sizeof(T))) < 0) {
            throw std::runtime_error(strerror(errno));
        }
#elif _WIN32
        DWORD dwRead;

        DWORD len = sizeof(T);

        if ((result = ReadFile(hPipe, &out, len, &dwRead, nullptr)) != FALSE) {
            if ((dwRead != len) || !result) {
                throw std::runtime_error("Pipe is closed!");
            }
        }
#endif
    }

    void ReadString(uint16_t length, std::u16string& out);
};

#endif  // NAMEDPIPEREADER_H
