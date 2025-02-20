#include "NamedPipeReader.h"

#include <cstdio>
#include <cstdint>
#include <iostream>

#include <sys/types.h>
#include <sys/stat.h>
#include <string.h>
#include <fcntl.h>
#include <unistd.h>


NamedPipeReader::NamedPipeReader() {
    std::cout << "Reader Constructor" << std::endl;

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
#endif
    std::cout << "Reader Constructor End" << std::endl;
}

NamedPipeReader::~NamedPipeReader() {
#ifdef __linux__
    close(hPipe);
#elif _WIN32
    DisconnectNamedPipe(hPipe);
#endif
}

MessageType NamedPipeReader::ReadMessageType() {
    uint16_t buffer;
#ifdef __linux__
    printf("Waiting for message...\n");
    if ((result = read(hPipe, &buffer, sizeof(buffer))) < 0) {
        printf("Error!\n");
        throw std::runtime_error(strerror(errno));
    }
    printf("Received: %u\n", buffer);
#elif _WIN32
    DWORD dwRead;
    DWORD len = sizeof(buffer);

    if ((result = ReadFile(hPipe, &buffer, len, &dwRead, NULL)) != FALSE) {
        /* do something with data in buffer */
        if ((dwRead != len) || !result) {
            throw std::runtime_error("Pipe is closed!");
        }
    }
#endif

    return (MessageType)buffer;
}

uint64_t NamedPipeReader::ReadConnectionMessage() {
    uint64_t buffer;
#ifdef __linux__
    if ((result = read(hPipe, &buffer, sizeof(buffer))) < 0) {
        throw std::runtime_error(strerror(errno));
    }
    printf("Client ID: %lu\n", buffer);
#elif _WIN32
    DWORD dwRead;
    DWORD len = sizeof(buffer);

    if ((result = ReadFile(hPipe, &buffer, len, &dwRead, NULL)) != FALSE) {
        /* do something with data in buffer */
        if ((dwRead != len) || !result) {
            throw std::runtime_error("Pipe is closed!");
        }
    }
#endif

    return buffer;
}

UpdateMessage NamedPipeReader::ReadUpdateMessage() {
    UpdateMessage buffer;
#ifdef __linux__
    if ((result = read(hPipe, &buffer, sizeof(buffer))) < 0) {
        throw std::runtime_error(strerror(errno));
    }
#elif _WIN32
    DWORD dwRead;

    DWORD len = sizeof(buffer);

    if ((result = ReadFile(hPipe, &buffer, len, &dwRead, NULL)) != FALSE) {
        /* do something with data in buffer */
        if ((dwRead != len) || !result) {
            throw std::runtime_error("Pipe is closed!");
        }
    }
#endif

    return buffer;
}

SpawnMessage NamedPipeReader::ReadSpawnMessage() {
    SpawnMessage buffer;
#ifdef __linux__
    if ((result = read(hPipe, &buffer, sizeof(buffer))) < 0) {
        throw std::runtime_error(strerror(errno));
    }
#elif _WIN32
    DWORD dwRead;

    DWORD len = sizeof(buffer);

    if ((result = ReadFile(hPipe, &buffer, len, &dwRead, NULL)) != FALSE) {
        /* do something with data in buffer */
        if ((dwRead != len) || !result) {
            throw std::runtime_error("Pipe is closed!");
        }
    }
#endif

    return buffer;
}
