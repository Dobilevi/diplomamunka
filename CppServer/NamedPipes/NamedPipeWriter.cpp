
#include "NamedPipeWriter.h"

#include <fcntl.h>

#include <chrono>
#include <thread>

#ifdef __linux__
#include <sys/stat.h>   // mkfifo
#include <sys/types.h>  // mkfifo
#endif

NamedPipeWriter::NamedPipeWriter() {
    std::this_thread::sleep_for(std::chrono::milliseconds(1000));

#ifdef __linux__
    writePipe =
        mkfifo("/tmp/CppPipe", S_IWUSR | S_IRUSR | S_IXUSR | S_IRGRP | S_IWGRP |
                                   S_IXGRP | S_IROTH | S_IWOTH | S_IXOTH);

    if ((writePipe = open("/tmp/CppPipe", O_WRONLY)) < 0) {
        printf("%s\n", std::strerror(errno));
    }
#elif _WIN32
    writePipe = CreateFile(TEXT("\\\\.\\pipe\\CppPipe"), GENERIC_WRITE, 0,
                           nullptr, OPEN_EXISTING, 0, nullptr);

    while (writePipe == INVALID_HANDLE_VALUE) {
        if (ConnectNamedPipe(writePipe, nullptr) !=
            FALSE)  // Wait for someone to connect to the pipe
            return;
    }
#endif
}

NamedPipeWriter::~NamedPipeWriter() {
#ifdef __linux__
    close(writePipe);
#elif _WIN32
    CloseHandle(writePipe);
#endif
}
