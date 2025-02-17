
#ifndef UUDP_H
#define UUDP_H

// uudp.h

#include <time.h>

typedef struct {
    long long  sequence;
    long long timestamp;
    char str[10];
} DataPacket;

// The reason this is in a seperate file is because I want to use this
// on the server size as well

#endif  // UUDP_H
