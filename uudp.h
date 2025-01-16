
#ifndef DIPLOMAMUNKA_UUDP_H
#define DIPLOMAMUNKA_UUDP_H

// uudp.h

#include <time.h>

typedef struct {
    long long  sequence;
    long long timestamp;
    char str[10];
} DataPacket;

// The reason this is in a seperate file is because I want to use this
// on the server size as well

#endif  // DIPLOMAMUNKA_UUDP_H
