
#include "Server.h"

#include <cassert>
#include <future>
#include <iostream>
#include <string>
#include <unistd.h>

#include <sys/types.h>

#include "NetworkHostConversion.h"

time_t GetTimestamp() {
    static const clockid_t clock_id = CLOCK_REALTIME;

    timespec time;
    clock_gettime(clock_id, &time);
    return time.tv_sec;
}

Server::Server() {
#ifdef _WIN32
    assert(result == NO_ERROR);
#endif

    udpWriter.SetSocket(udpReader.GetSocket());
    // Establish connection with the server instance

    // Check player timeout
    checkTimeoutAsyncTask = std::async(&Server::CheckTimeOutLoop, this);

    // Start forwarding messages to the clients
    receiveUpdateAsyncTask = std::async(&Server::ReceiveServerData, this);

    // Start listening for messages over the network
    receiveUdpAsyncTask = std::async(&Server::ReceiveUdp, this);

    receiveClientUpdateAsyncTask = std::async(&Server::ReceiveClientData, this);

    std::cout << "Server started!" << std::endl;
}

Server::~Server() {
    namedPipeWriter.Write(htons(EXIT));

#ifdef _WIN32
    WSACleanup();
#endif
}

[[noreturn]] void Server::ReceiveUdp() {
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
                namedPipeWriter.Write(htons(DISCONNECT));
                namedPipeWriter.Write(htonll(client.first));
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
        sleep(1);
        CheckTimeOut();
    }
}

[[noreturn]] void Server::ReceiveServerData() {
    MessageType messageType;
    Spawnable spawnable;
    ErrorType errorType;
    uint64_t clientId;
    uint64_t projectileId;
    uint16_t length;
    std::u16string playerName;
    SpawnProjectileMessage spawnProjectileMessage{};
    DespawnMessage despawnMessage{};
    UpdateMessage updateMessage{};
    SpawnMessage spawnMessage{};

    while (true) {
        namedPipeReader.Read(messageType);
//        std::cout << "messageType: " << ntohs(messageType) << std::endl;

        switch ((MessageType)ntohs(messageType)) {
            case MessageType::CONNECT: {
                namedPipeReader.Read(spawnMessage);
                namedPipeReader.Read(length);
                namedPipeReader.ReadString(ntohs(length), playerName);
                Player player = connectQueue.front();

                udpWriter.AddClient(player.ip, player.port, ntohll(spawnMessage.clientId));
                udpReader.AddClient(player.ip, player.port, ntohll(spawnMessage.clientId));

                lastReceivedMessageMapMutex.lock();
                {
                    lastReceivedMessageMap[ntohll(spawnMessage.clientId)] = GetTimestamp();
                }
                lastReceivedMessageMapMutex.unlock();

                connectQueue.pop();
                udpWriter.MulticastSpawnPlayerMessage(messageType, spawnMessage, playerName);
                break;
            }
            case MessageType::DISCONNECT: {
                namedPipeReader.Read(clientId);

                udpWriter.RemoveClient(ntohll(clientId));
                udpReader.RemoveClient(ntohll(clientId));

                udpWriter.MulticastMessage(messageType, clientId);
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
            case MessageType::SPAWN: {
                namedPipeReader.Read(clientId);
                namedPipeReader.Read(spawnMessage);
                if ((Spawnable)ntohs(spawnMessage.spawnable) == PLAYER) {
                    namedPipeReader.Read(length);
                    namedPipeReader.ReadString(ntohs(length), playerName);
                    udpWriter.SendSpawnPlayerUdpMessage(ntohll(clientId), messageType, spawnMessage, playerName);
                } else {
                    udpWriter.SendUdpMessage(ntohll(clientId), messageType, spawnMessage);
                }
                break;
            }
            case MessageType::_ERROR: {
                namedPipeReader.Read(clientId);
                namedPipeReader.Read(errorType);
                udpWriter.SendUdpMessage(ntohll(clientId), messageType, errorType);
                if ((ErrorType)ntohs(errorType) == ErrorType::SERVER_FULL) {
                    connectQueue.pop();
                }
                break;
            }
            case MessageType::EXIT: {
                std::cout << "EXIT!" << std::endl;
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
//                udpWriter.AddClient(udpReader.GetIPAddress(), udpReader.GetPortNumber(), udpReader.GetClientId());
                connectQueue.emplace(udpReader.GetIPAddress(), udpReader.GetPortNumber(), udpReader.GetClientIdHost(), udpReader.GetPlayerName());
//                namedPipeWriter.WriteConnectMessage(udpReader.GetMessageType(), udpReader.GetPlayerName().length(), udpReader.GetPlayerName());

                namedPipeWriter.Write(udpReader.GetMessageType());
                namedPipeWriter.WriteString(udpReader.GetPlayerName());
                break;
            }
            case DISCONNECT: {
//                namedPipeWriter.WriteMessage(udpReader.GetMessageType(), udpReader.GetClientId());
                namedPipeWriter.Write(udpReader.GetMessageType());
                namedPipeWriter.Write(udpReader.GetClientId());
                break;
            }
            case UPDATE: {
//                namedPipeWriter.WriteMessage(udpReader.GetMessageType(), udpReader.GetUpdateMessage());
                namedPipeWriter.Write(udpReader.GetMessageType());
                namedPipeWriter.Write(udpReader.GetUpdateMessage());

                lastReceivedMessageMapMutex.lock();
                {
                    lastReceivedMessageMap[ntohll(udpReader.GetUpdateMessage().clientId)] = GetTimestamp();
                }
                lastReceivedMessageMapMutex.unlock();
                break;
            }
            case SPAWN_PROJECTILE: {
//                namedPipeWriter.WriteMessage(udpReader.GetMessageType(), udpReader.GetSpawnMessage());
                namedPipeWriter.Write(udpReader.GetMessageType());
                namedPipeWriter.Write(udpReader.GetSpawnMessage());
                break;
            }
            case SPAWN: {
//                namedPipeWriter.WriteMessage(udpReader.GetMessageType(), udpReader.GetClientId());
                namedPipeWriter.Write(udpReader.GetMessageType());
                namedPipeWriter.Write(udpReader.GetClientId());
                break;
            }
            case _ERROR: {
                namedPipeWriter.Write(udpReader.GetMessageType());
                namedPipeWriter.Write(udpReader.GetErrorType());
                if (udpReader.GetErrorTypeHost() == ErrorType::OBJECT_DOES_NOT_EXIST) {
                    namedPipeWriter.Write(htonll(udpReader.GetSenderClientId()));
                    namedPipeWriter.Write(udpReader.GetClientId());
                }
            }
            default: {
                break;
            }
        }
    }
}
