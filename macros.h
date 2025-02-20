#ifndef MACROS_H
#define MACROS_H

#ifdef __linux__
#define INVALID_SOCKET (-1)
#define SOCKET_ERROR (-1)
typedef int SOCKET;
#elif _WIN32

#endif

#endif  // MACROS_H
