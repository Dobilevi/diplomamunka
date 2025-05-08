
#include "UDPReader.h"

#include <cassert>
#include <iostream>

#include <ws2tcpip.h>

#include "NetworkHostConversion.h"


UDPReader::UDPReader(uint16_t port) : buffer(256) {
    std::cout << "Port: " << port << std::endl;
    listenSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);

#ifdef __linux__
    assert(listenSocket != -1);
#elif _WIN32
    assert(listenSocket != INVALID_SOCKET);
#endif

    listenAddr.sin_addr.s_addr = inet_addr("0.0.0.0");
    listenAddr.sin_family = AF_INET;
    listenAddr.sin_port = htons(port);

    int result = bind(listenSocket, (sockaddr*)&listenAddr, sizeof(listenAddr));
#ifdef __linux__
    assert(result != -1);
#elif _WIN32
    assert(result != SOCKET_ERROR);
#endif
}

#ifdef __linux__
int UDPReader::GetSocket() const {
    return listenSocket;
}
#elif _WIN32
SOCKET UDPReader::GetSocket() const {
    return listenSocket;
}
#endif

void UDPReader::AddClient(const std::string& ip, const std::string& port, uint64_t clientId) {
    clientMutex.lock();

    connectionMap[ip][port] = clientId;
    packageIdMap[ip][port] = 0;
    clientAddressMap[clientId] = std::pair(ip, port);
    waitingMap[ip].erase(port);

    clientMutex.unlock();
}

void UDPReader::RemoveClient(uint64_t clientId) {
    clientMutex.lock();

    connectionMap.at(clientAddressMap.at(clientId).first).erase(clientAddressMap.at(clientId).second);
    packageIdMap.at(clientAddressMap.at(clientId).first).erase(clientAddressMap.at(clientId).second);
    clientAddressMap.erase(clientId);

    clientMutex.unlock();
}

bool UDPReader::IsConnected(const std::string& ip, const std::string& port, uint64_t clientId) {
    clientMutex.lock();

    bool isConnected = false;
    if (connectionMap.find(ip) != connectionMap.end()) {
        if (connectionMap.at(ip).find(port) != connectionMap.at(ip).end()) {
            isConnected = connectionMap.at(ip).at(port) == clientId;
        }
    }

    clientMutex.unlock();

    return isConnected;
}

bool UDPReader::IsConnected(const std::string& ip, const std::string& port) {
    clientMutex.lock();

    bool isConnected = false;
    if (connectionMap.find(ip) != connectionMap.end()) {
        isConnected = connectionMap.at(ip).find(port) != connectionMap.at(ip).end();
    }

    clientMutex.unlock();

    return isConnected;
}

void UDPReader::ReadUdpPackage() {
    FD_ZERO(&readfds);
    FD_SET(listenSocket, &readfds);

    if (select(listenSocket + 1, &readfds, nullptr, nullptr, nullptr) == -1) {
        return;
    }

    if (FD_ISSET(listenSocket, &readfds)) {
        static uint16_t size = 256;
        static UDP udp;
        udp = { size, new char[size] };

        socklen_t fromlen = sizeof(address);
        int rec = recvfrom(listenSocket, udp.buffer, size, 0, &address, &fromlen);
        if (rec == -1) {
            delete[] udp.buffer;
            return;
        }
        udp.size = rec;

        auto* sin = (struct sockaddr_in*)&address;
        udp.ip = inet_ntoa(sin->sin_addr);
        udp.port = std::to_string(ntohs(sin->sin_port));

        udpQueueMutex.lock();
        {
            udpQueue.push(udp);
        }
        udpQueueMutex.unlock();

    }
}

void UDPReader::ReadMessage() {
    static UDP udp;
    while (true) {
        udpQueueMutex.lock();
        if (!udpQueue.empty()) {
            break;
        }
        udpQueueMutex.unlock();
    }
    udp = udpQueue.front();
    udpQueue.pop();
    udpQueueMutex.unlock();

    ip = udp.ip;
    port = udp.port;

    buffer.SetBuffer(udp.buffer, udp.size);

    try {
        buffer.Read(packageIdNetwork);
        packageIdHost = ntohll(packageIdNetwork);
        buffer.Read(messageTypeNetwork);
        messageTypeHost = (MessageType)ntohs(messageTypeNetwork);

        if (messageTypeHost != CONNECT) {
            if (packageIdMap.find(ip) != packageIdMap.end()) {
                if (packageIdMap[ip].find(port) != packageIdMap[ip].end()) {
                    if (packageIdHost <= packageIdMap[ip][port]) {
                        std::cout << "Received past package!" << std::endl;
                        messageTypeHost = NONE;
                        return;
                    } else {
                        packageIdMap[ip][port] = packageIdHost;
                    }
                }
            }
        }

        switch (messageTypeHost) {
            case CONNECT: {
                buffer.Read(lengthNetwork);
                lengthHost = ntohs(lengthNetwork);
                buffer.ReadString(playerNameNetwork, lengthHost, maxPlayerNameLength);
                if (IsConnected(ip, port)) {
                    messageTypeHost = NONE;
                    return;
                }
                if (waitingMap.find(ip) != waitingMap.end()) {
                    if (waitingMap[ip].find(port) != waitingMap[ip].end()) {
                        messageTypeHost = NONE;
                        return;
                    }
                }
                waitingMap[ip][port] = true;
                //                m_clientId = Player::GetNextClientId();
                //                connectionMap[ip][port][m_clientId] = Player(ip, port, m_clientId, "Default name");

                break;
            }
            case DISCONNECT: {
                buffer.Read(m_clientIdNetwork);
                m_clientIdHost = ntohll(m_clientIdNetwork);
                if (!IsConnected(ip, port, m_clientIdHost)) {
                    messageTypeHost = NONE;
                    break;
                }
                //            connectionMap[ip].erase(port);
                //            packageIdMap[ip].erase(port);
                //            printf("Disconnected: %s:%s with id %llu\n", ip.c_str(), port.c_str(), m_clientIdHost);
                break;
            }
            case SPAWN_PROJECTILE: {
                buffer.Read(spawnMessageNetwork);
                m_clientIdHost = ntohll(spawnMessageNetwork.clientId);
                if (!IsConnected(ip, port, m_clientIdHost)) {
                    messageTypeHost = NONE;
                }
                break;
            }
            case UPDATE: {
                buffer.Read(updateMessageNetwork);
                m_clientIdHost = ntohll(updateMessageNetwork.clientId);
                if (!IsConnected(ip, port, m_clientIdHost)) {
                    printf("Not connected: %s: %llu\n", ip.c_str(),
                           m_clientIdHost);
                    for (auto ip : connectionMap) {
                        for (auto port : ip.second) {
                            printf("%s:%s -> %llu\n", ip.first.c_str(),
                                   port.first.c_str(), port.second);
                        }
                    }
                    messageTypeHost = NONE;
                }
                break;
            }
            case RESPAWN_ACK: {
                buffer.Read(m_clientIdNetwork);
                m_clientIdHost = ntohll(m_clientIdNetwork);

                if (!IsConnected(ip, port, m_clientIdHost)) {
                    messageTypeHost = NONE;
                    break;
                }
                break;
            }
            case ERROR_OBJECT_DOES_NOT_EXIST: {
                buffer.Read(errorTypeNetwork);
                buffer.Read(m_clientIdNetwork);
                break;
            }
            default: {
                messageTypeHost = NONE;
                break;
            }
        }
    } catch (const std::runtime_error& error) {
        messageTypeHost = NONE;
    }

//    } else {
//        // Timeout
//    }
}

uint64_t UDPReader::GetSenderClientId() const {
    return connectionMap.at(ip).at(port);
}

uint64_t UDPReader::GetPackageId() const {
    return packageIdNetwork;
}

uint16_t UDPReader::GetMessageType() const {
    return messageTypeNetwork;
}

MessageType UDPReader::GetMessageTypeHost() const {
    return messageTypeHost;
}

uint16_t UDPReader::GetErrorType() const {
    return errorTypeNetwork;
}

const std::string& UDPReader::GetIPAddress() const {
    return ip;
}

const std::string& UDPReader::GetPortNumber() const {
    return port;
}

uint64_t UDPReader::GetClientId() const {
    return m_clientIdNetwork;
}

uint64_t UDPReader::GetClientIdHost() const {
    return m_clientIdHost;
}

const std::u16string& UDPReader::GetPlayerName() const {
    return playerNameNetwork;
}

const UpdateMessage& UDPReader::GetUpdateMessage() const {
    return updateMessageNetwork;
}

const SpawnProjectileMessage& UDPReader::GetSpawnMessage() const {
    return spawnMessageNetwork;
}
