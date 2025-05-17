
#include "NamedPipeReader.h"

#include <fcntl.h>

#include <cstdint>
#include <iostream>

#ifdef __linux__
#include <arpa/inet.h>
#include <sys/stat.h>
#include <unistd.h>

#include <cstdio>
#endif

NamedPipeReader::NamedPipeReader() {
#ifdef __linux__
    listenPipe = mkfifo("/tmp/CsharpPipe", S_IWUSR | S_IRUSR | S_IXUSR |
                                               S_IRGRP | S_IWGRP | S_IXGRP |
                                               S_IROTH | S_IWOTH | S_IXOTH);

    sleep(1);

    if ((listenPipe = open("/tmp/CsharpPipe", O_RDONLY)) < 0) {
        printf("%s\n", std::strerror(errno));
        return;
    }
#elif _WIN32
    listenPipe =
        CreateNamedPipe(TEXT("\\\\.\\pipe\\CsharpPipe"), PIPE_ACCESS_DUPLEX,
                        PIPE_TYPE_BYTE | PIPE_READMODE_BYTE |
                            PIPE_WAIT,  // FILE_FLAG_FIRST_PIPE_INSTANCE is not
                                        // needed but forces CreateNamedPipe(..)
                                        // to fail if the pipe already exists...
                        1,
                        UINT16_MAX,
                        UINT16_MAX,
                        NMPWAIT_USE_DEFAULT_WAIT, nullptr);

    while (listenPipe != INVALID_HANDLE_VALUE) {
        if (ConnectNamedPipe(listenPipe, nullptr) !=
            FALSE) {  // Wait for someone to connect to the pipe
            return;
        }
    }
#endif
}

NamedPipeReader::~NamedPipeReader() {
#ifdef __linux__
    close(listenPipe);
#elif _WIN32
    DisconnectNamedPipe(listenPipe);
#endif
}

uint16_t NamedPipeReader::ReadPort() {
    uint16_t out;

#ifdef __linux__
    if ((result = read(listenPipe, &out, sizeof(uint16_t))) < 0) {
        throw std::runtime_error(std::strerror(errno));
    }
#elif _WIN32
    DWORD dwRead;

    DWORD len = sizeof(uint16_t);

    if ((result = ReadFile(listenPipe, &out, len, &dwRead, nullptr)) != FALSE) {
        if ((dwRead != len) || !result) {
            throw std::runtime_error("Pipe is closed!");
        }
    }
#endif

    return ntohs(out);
}

void NamedPipeReader::ReadString(uint16_t length, std::u16string& out) {
    char16_t buffer[64];

#ifdef __linux__
    if ((result = read(listenPipe, &buffer, sizeof(char16_t) * length)) < 0) {
        throw std::runtime_error(std::strerror(errno));
    }
#elif _WIN32
    DWORD dwRead;

    DWORD len = sizeof(char16_t) * length;

    if ((result = ReadFile(listenPipe, &buffer, len, &dwRead, nullptr)) !=
        FALSE) {
        if ((dwRead != len) || !result) {
            throw std::runtime_error("Pipe is closed!");
        }
    }
#endif
    const char16_t nul = 0;
    std::memcpy(buffer + length, &nul, sizeof(char16_t));

    out = buffer;
}
