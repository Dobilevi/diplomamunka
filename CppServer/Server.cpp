
#include "Server.h"

#include <cassert>
#include <ctime>
#include <future>
#include <iostream>
#include <string>

#ifdef __linux__
#include <unistd.h>
#include <sys/types.h> // TODO: needed?
#elif _WIN32
#include <windows.h>
#endif

#include "NetworkHostConversion.h"

std::time_t GetTimestamp() {
    std::time_t time;

    std::time(&time);

    return time;
}

Server::Server() {
#ifdef _WIN32
    assert(result == NO_ERROR);
#endif
    struct sockaddr_in sockAddr{};

    uint16_t port = namedPipeReader.ReadPort();
    std::cout << "Listening on port: " << port << std::endl;
    serverSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);

#ifdef __linux__
    assert(serverSocket != -1);
#elif _WIN32
    assert(serverSocket != INVALID_SOCKET);
#endif

    sockAddr.sin_addr.s_addr = inet_addr("0.0.0.0");
    sockAddr.sin_family = AF_INET;
    sockAddr.sin_port = htons(port);

    result = bind(serverSocket, (sockaddr*)&sockAddr, sizeof(sockAddr));
#ifdef __linux__
    assert(result != -1);
#elif _WIN32
    assert(result != SOCKET_ERROR);
#endif

    udpWriter.SetSocket(serverSocket);
    udpReader.SetSocket(serverSocket);

    // Check player timeout
    checkTimeoutAsyncTask = std::async(&Server::CheckTimeOutLoop, this);

    // Start forwarding messages to the clients
    receiveUpdateAsyncTask = std::async(&Server::ReceiveServerData, this);

    // Start listening for arriving messages over the network
    receiveUdpAsyncTask = std::async(&Server::ReceiveUDPMessages, this);

    // Start processing the UDP messages
    receiveClientUpdateAsyncTask = std::async(&Server::ReceiveClientData, this);

    std::cout << "Server started" << std::endl;
}

Server::~Server() {
    namedPipeWriterMutex.lock();
    {
        namedPipeWriter.Write(htons(EXIT));
    }
    namedPipeWriterMutex.unlock();

#ifdef __linux__
    close(serverSocket);
#elif _WIN32
    closesocket(serverSocket);
    WSACleanup();
#endif
}

[[noreturn]] void Server::ReceiveUDPMessages() {
    while (true) {
        udpReader.ReadUdpPackage();
    }
}

void Server::CheckTimeOut() {
    time_t timeStamp = GetTimestamp();
    std::vector<uint64_t> clientsToRemove{};

    lastReceivedMessageMapMutex.lock();
    {
        for (const auto& client : lastReceivedMessageMap) {
            if ((timeStamp - client.second) > timeOutLimit) {
                clientsToRemove.emplace_back(client.first);

                namedPipeWriterMutex.lock();
                {
                    namedPipeWriter.Write(htons(DISCONNECT));
                    namedPipeWriter.Write(htonll(client.first));
                }
                namedPipeWriterMutex.unlock();
            }
        }

        for (uint64_t clientId : clientsToRemove) {
            lastReceivedMessageMap.erase(clientId);
        }
    }
    lastReceivedMessageMapMutex.unlock();
}

[[noreturn]] void Server::CheckTimeOutLoop() {
    while (true) {
#ifdef __linux__
        sleep(1);
#elif _WIN32
        Sleep(1000);
#endif
        CheckTimeOut();
    }
}

[[noreturn]] void Server::ReceiveServerData() {
    // Storage for raw data
    MessageType messageType;
    uint64_t clientId;
    uint16_t length;
    std::u16string playerName;
    SpawnProjectileMessage spawnProjectileMessage{};
    DespawnMessage despawnMessage{};
    UpdateMessage updateMessage{};
    SpawnMessage spawnMessage{};

    while (true) {
        namedPipeReader.Read(messageType);

        switch (ntohs(messageType)) {
            case MessageType::CONNECT: {
                namedPipeReader.Read(spawnMessage);
                namedPipeReader.Read(length);
                namedPipeReader.ReadString(ntohs(length), playerName);

                Player player = connectQueue.front();
                connectQueue.pop();

                if (!udpWriter.AddClient(player.ip, player.port, ntohll(spawnMessage.clientId))) {
                    break;
                }
                udpReader.AddClient(player.ip, player.port, ntohll(spawnMessage.clientId));

                lastReceivedMessageMapMutex.lock();
                {
                    lastReceivedMessageMap[ntohll(spawnMessage.clientId)] = GetTimestamp();
                }
                lastReceivedMessageMapMutex.unlock();

                udpWriter.MulticastSpawnPlayerMessage(messageType, spawnMessage, length, playerName);
                break;
            }
            case MessageType::DISCONNECT: {
                namedPipeReader.Read(clientId);

                uint64_t clientIdHost = ntohll(clientId);
                udpReader.RemoveClient(clientIdHost);

                udpWriter.MulticastMessage(messageType, clientId);

                udpWriter.RemoveClient(clientIdHost);

                break;
            }
            case MessageType::SPAWN_PROJECTILE: {
                namedPipeReader.Read(spawnProjectileMessage);
                udpWriter.MulticastMessage(messageType, spawnProjectileMessage);

                break;
            }
            case MessageType::DESPAWN: {
                namedPipeReader.Read(despawnMessage);
                udpWriter.MulticastMessage(messageType, despawnMessage);

                break;
            }
            case MessageType::UPDATE: {
                namedPipeReader.Read(updateMessage);
                udpWriter.MulticastMessage(messageType, updateMessage);

                break;
            }
            case MessageType::INITIALIZE_PLAYER:
            case MessageType::SPAWN_PLAYER: {
                namedPipeReader.Read(clientId);
                namedPipeReader.Read(spawnMessage);
                namedPipeReader.Read(length);
                namedPipeReader.ReadString(ntohs(length), playerName);

                udpWriter.SendSpawnPlayerUdpMessage(ntohll(clientId), messageType, spawnMessage, playerName);

                break;
            }
            case MessageType::INITIALIZE_ENEMY:
            case MessageType::SPAWN_ENEMY: {
                namedPipeReader.Read(clientId);
                namedPipeReader.Read(spawnMessage);

                udpWriter.SendUdpMessage(ntohll(clientId), messageType, spawnMessage);

                break;
            }
            case MessageType::INITIALIZE_PROJECTILE: {
                namedPipeReader.Read(clientId);
                namedPipeReader.Read(spawnProjectileMessage);

                udpWriter.SendUdpMessage(ntohll(clientId), messageType, spawnProjectileMessage);

                break;
            }
            case MessageType::ERROR_SERVER_FULL: {
                namedPipeReader.Read(clientId);
                Player player;

                connectQueueMutex.lock();
                {
                    player = connectQueue.front();
                    connectQueue.pop();
                }
                connectQueueMutex.unlock();

                udpWriter.SendErrorMessage(player.ip, player.port, messageType);

                break;
            }
            case MessageType::EXIT: {
                exit(0);
            }
            default: {
                break;
            }
        }
    }
}

[[noreturn]] void Server::ReceiveClientData() {
    while (true) {
        udpReader.ReadMessage();

        switch (udpReader.GetMessageTypeHost()) {
            case CONNECT: {
                connectQueueMutex.lock();
                {
                    connectQueue.emplace(udpReader.GetIPAddress(), udpReader.GetPortNumber(), udpReader.GetClientIdHost(), udpReader.GetPlayerName());
                }
                connectQueueMutex.unlock();

                namedPipeWriterMutex.lock();
                {
                    namedPipeWriter.Write(udpReader.GetMessageType());
                    namedPipeWriter.WriteString(udpReader.GetPlayerName());
                }
                namedPipeWriterMutex.unlock();

                break;
            }
            case DISCONNECT: {
                namedPipeWriterMutex.lock();
                {
                    namedPipeWriter.Write(udpReader.GetMessageType());
                    namedPipeWriter.Write(udpReader.GetClientId());
                }
                namedPipeWriterMutex.unlock();

                break;
            }
            case UPDATE: {
                namedPipeWriterMutex.lock();
                {
                    namedPipeWriter.Write(udpReader.GetMessageType());
                    namedPipeWriter.Write(udpReader.GetUpdateMessage());
                }
                namedPipeWriterMutex.unlock();

                lastReceivedMessageMapMutex.lock();
                {
                    lastReceivedMessageMap[ntohll(udpReader.GetUpdateMessage().clientId)] = GetTimestamp();
                }
                lastReceivedMessageMapMutex.unlock();

                break;
            }
            case SPAWN_PROJECTILE: {
                namedPipeWriterMutex.lock();
                {
                    namedPipeWriter.Write(udpReader.GetMessageType());
                    namedPipeWriter.Write(udpReader.GetSpawnMessage());
                }
                namedPipeWriterMutex.unlock();

                break;
            }
            case RESPAWN_ACK: {
                namedPipeWriterMutex.lock();
                {
                    namedPipeWriter.Write(udpReader.GetMessageType());
                    namedPipeWriter.Write(udpReader.GetClientId());
                }
                namedPipeWriterMutex.unlock();

                break;
            }
            case ERROR_OBJECT_DOES_NOT_EXIST: {
                namedPipeWriterMutex.lock();
                {
                    namedPipeWriter.Write(udpReader.GetMessageType());
                    namedPipeWriter.Write(udpReader.GetErrorType());
                    namedPipeWriter.Write(htonll(udpReader.GetSenderClientId()));
                    namedPipeWriter.Write(udpReader.GetClientId());
                }
                namedPipeWriterMutex.unlock();

                break;
            }
            default: {
                break;
            }
        }
    }
}
