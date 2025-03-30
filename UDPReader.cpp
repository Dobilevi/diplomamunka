
#include "UDPReader.h"

#include <cassert>
#include <cstring>
#include <ws2tcpip.h>

#include <iostream>

UDPReader::UDPReader() {
    listenSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);

    assert(listenSocket != INVALID_SOCKET);

    listenAddr.sin_addr.s_addr = inet_addr("0.0.0.0");
    listenAddr.sin_family = AF_INET;
    listenAddr.sin_port = htons(SERVER_PORT);

    int result = bind(listenSocket, (sockaddr*)&listenAddr, sizeof(listenAddr));
    assert(result != SOCKET_ERROR);

//    setsockopt(listenSocket, SOL_SOCKET, SO_RCVBUF, "65536", 5);
}

SOCKET UDPReader::GetSocket() const {
    return listenSocket;
}

bool UDPReader::IsConnected(const std::string& ip, const std::string& port, uint64_t clientId) const {
    if (connectionMap.find(ip) != connectionMap.end()) {
        if (connectionMap.at(ip).find(port) != connectionMap.at(ip).end()) {
//            return connectionMap.at(ip).at(port).find(clientId) != connectionMap.at(ip).at(port).end();
            return true;
        } else {
            return false;
        }
    } else {
        return false;
    }
}

void UDPReader::ReadMessage() {
    FD_ZERO(&readfds);
    FD_SET(listenSocket, &readfds);

    socklen_t fromlen = sizeof(address);

    int res = select(listenSocket + 1, &readfds, nullptr, nullptr, nullptr);

    if (FD_ISSET(listenSocket, &readfds)) {
        int received = recvfrom(listenSocket, buffer, 256, 0, &address, &fromlen);
        auto *sin = (struct sockaddr_in*)&address;
        ip = inet_ntoa(sin->sin_addr);
        port = std::to_string(ntohs(sin->sin_port));

        std::memcpy(&packageId, buffer, sizeof(uint64_t));
        std::memcpy(&messageType, buffer + sizeof(uint64_t), sizeof(MessageType));

        switch (messageType) {
            case Connect: {
                std::memcpy(&m_clientId, buffer + sizeof(uint64_t) + sizeof(MessageType), sizeof(SpawnMessage));
                if (IsConnected(ip, port, m_clientId)) {
                    messageType = None;
                }
                m_clientId = Player::GetNextClientId();
                connectionMap[ip][port][m_clientId] = Player(ip, port, m_clientId, "Default name");

                printf("Connected: %s:%s with id %llu\n", ip.c_str(), port.c_str(), m_clientId);
                break;
            }
            case Disconnect: {
                std::memcpy(&m_clientId, buffer + sizeof(uint64_t) + sizeof(MessageType), sizeof(SpawnMessage));
                if (!IsConnected(ip, port, m_clientId)) {
                    messageType = None;
                }
                connectionMap[ip].erase(port);
                printf("Disconnected: %s:%s with id %llu\n", ip.c_str(), port.c_str(), m_clientId);
                break;
            }
            case Spawn: {
                std::memcpy(&spawnMessage, buffer + sizeof(uint64_t) + sizeof(MessageType), sizeof(SpawnMessage));
                if (!IsConnected(ip, port, m_clientId)) {
                    messageType = None;
                }
                break;
            }
            case Update: {
                std::memcpy(&updateMessage, buffer + sizeof(uint64_t) + sizeof(MessageType), sizeof(UpdateMessage));
                if (!IsConnected(ip, port, m_clientId)) {
                    printf("Not connected: %s: %llu\n", ip.c_str(), m_clientId);
                    for (auto a : connectionMap) {
                        for (auto b : a.second) {
                            for (auto c : b.second) {
                                printf("%s:%s -> %llu\n", c.second.ip.c_str(), c.second.port.c_str(), c.second.clientId);
                            }
                        }
                    }
                    messageType = None;
                }
                break;
            }
            default: {
                break;
            }
        }

    } else {
        // Timeout
    }
}

uint64_t UDPReader::GetPackageId() const {
    return packageId;
}

MessageType UDPReader::GetMessageType() const {
    return messageType;
}

const std::string& UDPReader::GetIPAddress() const {
    return ip;
}

const std::string& UDPReader::GetPortNumber() const {
    return port;
}

uint64_t UDPReader::GetClientId() const {
    return m_clientId;
}

const UpdateMessage& UDPReader::GetUpdateMessage() const {
    return updateMessage;
}

const SpawnMessage& UDPReader::GetSpawnMessage() const {
    return spawnMessage;
}
