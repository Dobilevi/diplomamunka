
#include "UDPWriter.h"

#include <cassert>
#include <iostream>


UDPWriter::UDPWriter() {
    addrinfo hints{}, *servinfo;
    memset(&hints, 0, sizeof(hints));
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_DGRAM;

    int rv = getaddrinfo("127.0.0.1", "5000", &hints, &servinfo);
    for(sendP = servinfo; sendP != NULL; sendP = sendP->ai_next) {
        if ((sendSocket = socket(sendP->ai_family, sendP->ai_socktype,
                                 sendP->ai_protocol)) == -1) {
            perror("talker: socket");
            continue;
        }

        break;
    }

    assert(sendSocket != INVALID_SOCKET);
}

//void UDPWriter::SendMessageTo(MessageType messageType, ) {
//    FD_ZERO(&writefds);
//    FD_SET(sendSocket, &writefds);
//
//    select(sendSocket + 1, nullptr, &writefds, nullptr, nullptr);
//
//    if (FD_ISSET(sendSocket, &writefds)) {
//        int result = sendto(sendSocket, reinterpret_cast<char*>(buffer), sizeof(float) * 2, 0, sendP->ai_addr, sendP->ai_addrlen);
//
//        if (result == -1) {
//            printf("Error: %d\n", errno);
//        } else {
//            printf("Sent!\n");
//        }
//    } else {
//        // Timed out
//    }
//}
