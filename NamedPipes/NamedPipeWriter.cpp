
#include "NamedPipeWriter.h"

#include <chrono>
#include <cstring>
#include <iostream>
#include <thread>

#include <sys/types.h>  // mkfifo
#include <fcntl.h>

#ifdef __linux__
#include <sys/stat.h>   // mkfifo
#endif

NamedPipeWriter::NamedPipeWriter() {
    std::this_thread::sleep_for(std::chrono::milliseconds(1000));

#ifdef __linux__
    hPipe = mkfifo("/tmp/CppPipe", S_IWUSR | S_IRUSR | S_IXUSR | S_IRGRP | S_IWGRP | S_IXGRP | S_IROTH | S_IWOTH | S_IXOTH);
    printf("%d\n", hPipe);
    printf("%s\n", strerror(errno));

    if ((hPipe = open("/tmp/CppPipe", O_WRONLY)) < 0) {
        printf("%s\n", strerror(errno));
    }
#elif _WIN32
    hPipe = CreateFile(TEXT("\\\\.\\pipe\\CppPipe"),
                   GENERIC_WRITE,
                   0,
                   nullptr,
                   OPEN_EXISTING,
                   0,
                   nullptr);

    while (hPipe == INVALID_HANDLE_VALUE) {
        if (ConnectNamedPipe(hPipe, nullptr) !=
            FALSE)  // wait for someone to connect to the pipe
            return;
    }
#endif
}

NamedPipeWriter::~NamedPipeWriter() {
#ifdef __linux__
    close(hPipe);
#elif _WIN32
    CloseHandle(hPipe);
#endif
}
