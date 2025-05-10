
#include "UDPWriter.h"

#include <iostream>


#ifdef __linux__
void UDPWriter::SetSocket(int newSocket) {
    writeSocket = newSocket;
    FD_ZERO(&currentwritefds);
    FD_SET(writeSocket, &currentwritefds);
}
#elif _WIN32
void UDPWriter::SetSocket(SOCKET newSocket) {
    writeSocket = newSocket;
    FD_ZERO(&currentwritefds);
    FD_SET(writeSocket, &currentwritefds);
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
    } else {
        std::cout << "Error when adding client, error code: " << rv << std::endl; // TODO
    }
}

void UDPWriter::RemoveClient(uint64_t clientId) {
    addrinfoMap.erase(clientId);
    packageIdMap.erase(clientId);
}

void UDPWriter::SendUDPPackage(addrinfo* adddr) {
    FD_ZERO(&writefds);
    FD_SET(writeSocket, &writefds);

    if (select(writeSocket + 1, nullptr, &writefds, nullptr, nullptr) == -1) {
        // An error occurred
        return;
    }

    if (FD_ISSET(socket, &writefds)) {
        sendto(writeSocket, buffer.GetBuffer(), buffer.GetSize(), 0, adddr->ai_addr, adddr->ai_addrlen);
    }
}

void UDPWriter::MulticastUDPPackage() {
    uint64_t packageId;
    for (auto addr_info : addrinfoMap) {
        packageId = htonll(++packageIdMap[addr_info.first]);
        std::memcpy(buffer.GetBuffer(), &packageId, sizeof(uint64_t));

        SendUDPPackage(addr_info.second);
    }
}

void UDPWriter::SendErrorMessage(const std::string& ip, const std::string& port, MessageType messageType) {
    addrinfo hints{}, *servinfo;
    memset(&hints, 0, sizeof(hints));
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_DGRAM;
    hints.ai_protocol = IPPROTO_UDP;

    if (getaddrinfo(ip.c_str(), port.c_str(), &hints, &servinfo) != 0) {
        // An error occurred
        return;
    }

    buffer.Reset();

    buffer.Write(htonll((uint64_t)0));
    buffer.Write(messageType);

    SendUDPPackage(servinfo);
}
