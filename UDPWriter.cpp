
#include "UDPWriter.h"

#include <iostream>


UDPWriter::UDPWriter() {
    FD_ZERO(&currentwritefds);
}

void UDPWriter::SetSocket(SOCKET newSocket) {
    socket = newSocket;
    FD_SET(socket, &currentwritefds);
}

void UDPWriter::AddClient(const std::string& ip, const std::string& port, uint64_t clientId) {
    addrinfo hints{}, *servinfo;
    memset(&hints, 0, sizeof(hints));
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_DGRAM;
    hints.ai_protocol = IPPROTO_UDP;

    int rv = getaddrinfo(ip.c_str(), port.c_str(), &hints, &servinfo);

    if (rv == 0) {
        addrinfoMap[clientId] = servinfo;
        packageIdMap[clientId] = 0;
        clientMap[ip][port] = clientId;
        std::cout <<"clientId: " << clientId << std::endl;
    } else {
        std::cout << "Error when adding client, error code: " << rv << std::endl;
    }
}

void UDPWriter::RemoveClient(const std::string& ip, const std::string& port, uint64_t clientId) {
    std::cout << "Removing: client: " << clientId << ", clientId: " << clientMap[ip][port] << std::endl;
    addrinfoMap.erase(clientId);
    packageIdMap.erase(clientId);
    clientMap.at(ip).erase(port);

    for (auto addrinfo : addrinfoMap) {
        std::cout << "clientList: " << addrinfo.first << std::endl;
    }
}
