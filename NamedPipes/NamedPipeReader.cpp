
#include "NamedPipeReader.h"

#include <cstdint>
#include <cstdio>
#include <cstring>
#include <iostream>

#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <unistd.h>


NamedPipeReader::NamedPipeReader() {
#ifdef __linux__
    hPipe = mkfifo("/tmp/CsharpPipe", S_IWUSR | S_IRUSR | S_IXUSR | S_IRGRP | S_IWGRP | S_IXGRP | S_IROTH | S_IWOTH | S_IXOTH);
    printf("%d\n", hPipe);
    printf("%s\n", strerror(errno));
    sleep(1);
    if ((hPipe = open("/tmp/CsharpPipe", O_RDONLY)) < 0) {
        printf("%s\n", strerror(errno));
        return;
    }
#elif _WIN32
    hPipe = CreateNamedPipe(TEXT("\\\\.\\pipe\\CsharpPipe"),
                            PIPE_ACCESS_DUPLEX,
                            PIPE_TYPE_BYTE | PIPE_READMODE_BYTE | PIPE_WAIT,   // FILE_FLAG_FIRST_PIPE_INSTANCE is not needed but forces CreateNamedPipe(..) to fail if the pipe already exists...
                            1,
                            1024 * 16, // TODO
                            1024 * 16,
                            NMPWAIT_USE_DEFAULT_WAIT,
                            nullptr);

    while (hPipe != INVALID_HANDLE_VALUE) {
        if (ConnectNamedPipe(hPipe, nullptr) != FALSE) {  // Wait for someone to connect to the pipe
            return;
        }
    }
#endif
}

NamedPipeReader::~NamedPipeReader() {
#ifdef __linux__
    close(hPipe);
#elif _WIN32
    DisconnectNamedPipe(hPipe);
#endif
}

void NamedPipeReader::ReadString(uint16_t length, std::u16string& out) {
    char16_t buffer[256];
#ifdef __linux__
    if ((result = read(hPipe, &buffer, sizeof(char16_t) * length)) < 0) {
        throw std::runtime_error(strerror(errno));
    }
#elif _WIN32
    DWORD dwRead;

    DWORD len = sizeof(char16_t) * length;

    if ((result = ReadFile(hPipe, &buffer, len, &dwRead, nullptr)) != FALSE) {
        if ((dwRead != len) || !result) {
            throw std::runtime_error("Pipe is closed!");
        }
    }
#endif
    buffer[length] = '\0';

    out = buffer;
}
