
#include "UDPWriter.h"

#include <iostream>


UDPWriter::UDPWriter() {
    FD_ZERO(&currentwritefds);
}

#ifdef __linux__
void UDPWriter::SetSocket(int newSocket) {
    socket = newSocket;
    FD_SET(socket, &currentwritefds);
}
#elif _WIN32
void UDPWriter::SetSocket(SOCKET newSocket) {
    socket = newSocket;
    FD_SET(socket, &currentwritefds);
}
#endif

void UDPWriter::AddClient(const std::string& ip, const std::string& port, uint64_t clientId) {
    std::cout << ip << ":" << port << " " << clientId << std::endl;
    addrinfo hints{}, *servinfo;
    memset(&hints, 0, sizeof(hints));
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_DGRAM;
    hints.ai_protocol = IPPROTO_UDP;

    int rv = getaddrinfo(ip.c_str(), port.c_str(), &hints, &servinfo);

    if (rv == 0) {
        addrinfoMap[clientId] = servinfo;
        packageIdMap[clientId] = 0;
        std::cout <<"clientId: " << clientId << std::endl;
    } else {
        std::cout << "Error when adding client, error code: " << rv << std::endl;
    }
}

void UDPWriter::RemoveClient(uint64_t clientId) {
    std::cout << "Removing: client: " << clientId << ", clientId: " << clientId << std::endl;
    addrinfoMap.erase(clientId);
    packageIdMap.erase(clientId);
}

void UDPWriter::SendErrorMessage(const std::string& ip, const std::string& port, MessageType messageType) {
    addrinfo hints{}, *servinfo;
    memset(&hints, 0, sizeof(hints));
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_DGRAM;
    hints.ai_protocol = IPPROTO_UDP;

    int rv = getaddrinfo(ip.c_str(), port.c_str(), &hints, &servinfo);

    if (rv == 0) {

    }

    buffer.Reset();

    buffer.Write(htonll((uint64_t)0));
    buffer.Write(messageType);

    FD_ZERO(&writefds);
    FD_SET(socket, &writefds);

    select(socket + 1, nullptr, &writefds, nullptr, nullptr);

    if (FD_ISSET(socket, &writefds)) {
        int result = sendto(socket, buffer.GetBuffer(), buffer.GetSize(), 0, servinfo->ai_addr, servinfo->ai_addrlen);

        if (result == -1) {
            printf("SendUdpMessage Error: %d\n", errno);
        }
    } else {
        // Timed out
        printf("SendUdpMessage TIMEOUT: socket: %llu!\n", socket);
    }
}
