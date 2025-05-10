
#include "NetworkHostConversion.h"

#include <algorithm>

#if __BIG_ENDIAN__

uint64_t htonll(uint64_t hostlonglong) {
    return hostlonglong;
}

uint64_t ntohll(uint64_t netlonglong) {
    return netlonglong;
}

#else

uint64_t htonll(uint64_t hostlonglong) {
    std::reverse((char*)&hostlonglong, ((char*)&hostlonglong) + sizeof(uint64_t));

    return hostlonglong;
}

uint64_t ntohll(uint64_t netlonglong) {
    return htonll(netlonglong);
}

#endif
