#include "NamedPipeWriter.h"

#include <iostream>
#include <chrono>
#include <thread>

#include <sys/types.h>  // mkfifo
#include <sys/stat.h>   // mkfifo
#include <string.h>
#include <fcntl.h>

NamedPipeWriter::NamedPipeWriter() {
    std::cout << "Writer Constructor" << std::endl;
//    hPipe = CreateNamedPipe(
//        TEXT("\\\\.\\pipe\\Pipe"), PIPE_ACCESS_DUPLEX,
//        PIPE_TYPE_BYTE | PIPE_READMODE_BYTE |
//            PIPE_WAIT,  // FILE_FLAG_FIRST_PIPE_INSTANCE is not needed but forces CreateNamedPipe(..) to fail if the pipe already exists...
//        1, 1024 * 16, 1024 * 16, NMPWAIT_USE_DEFAULT_WAIT, NULL);

    std::this_thread::sleep_for(std::chrono::milliseconds(1000)); // TODO: hack

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
                   NULL,
                   OPEN_EXISTING,
                   0,
                   NULL);

    while (hPipe == INVALID_HANDLE_VALUE) {
        if (ConnectNamedPipe(hPipe, NULL) !=
            FALSE)  // wait for someone to connect to the pipe
            return;
    }
#endif
    std::cout << "Writer Constructor End" << std::endl;
}

NamedPipeWriter::~NamedPipeWriter() {
//    DisconnectNamedPipe(hPipe);
#ifdef __linux__
    close(hPipe);
#elif _WIN32
    CloseHandle(hPipe);
#endif
}
