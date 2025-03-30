
#include "Server.h"

#include <cassert>
#include <future>
#include <iostream>
#include <sstream>
#include <string>
#include <cstring>
#include <iostream>
#include <cerrno>

#ifdef _WIN32
#include <winsock2.h>
#include <ws2tcpip.h>
#endif

#include <fcntl.h>

//#include <stdlib.h>
#include <unistd.h>
#include <sys/stat.h>
#include <sys/types.h>

Server::Server() {
#ifdef _WIN32
    assert(result == NO_ERROR);
#endif

    udpWriter.SetSocket(udpReader.GetSocket());
    // Establish connection with the server instance

    // Start forwarding messages to the clients
    receiveUpdateAsyncTask = std::async(&Server::ReceiveServerData, this);

    // Start listening for messages over the network
    receiveClientUpdateAsyncTask = std::async(&Server::ReceiveClientData, this);

    std::cout << "Server Constructor End" << std::endl;
}

Server::~Server() {
    namedPipeWriter.WriteMessage(0, Exit, nullptr);

#ifdef __linux__

#elif _WIN32
    WSACleanup();
#endif
}

[[noreturn]] void Server::ReceiveServerData() {
    MessageType messageType;
    uint64_t clientId;
    SpawnMessage spawnMessage{};
    UpdateMessage updateMessage{};

    while (true) {
        messageType = namedPipeReader.ReadMessageType();

        switch (messageType) {
            case MessageType::Connect: {
                clientId = namedPipeReader.ReadUint64();
                udpWriter.MulticastMessage(messageType, clientId);
                break;
            }
            case MessageType::Disconnect: {
                printf("DISCONNECT MESSAGE!\n");
                clientId = namedPipeReader.ReadUint64();
                udpWriter.MulticastMessage(messageType, clientId);
                break;
            }
            case MessageType::Spawn: {
                spawnMessage = namedPipeReader.ReadSpawnMessage();
                udpWriter.MulticastMessage(messageType, spawnMessage);
                break;
            }
            case MessageType::Update: {
                updateMessage = namedPipeReader.ReadUpdateMessage();
                udpWriter.MulticastMessage(messageType, updateMessage);
                break;
            }
            case MessageType::SetStartPosition: {
                clientId = namedPipeReader.ReadUint64();
                updateMessage = namedPipeReader.ReadUpdateMessage();
                udpWriter.SendUdpMessage(clientId, messageType, updateMessage);
                break;
            }
            case MessageType::Exit: {
                exit(0);
            }
            default: {
                // TODO
                break;
            }
        }
    }
}

[[noreturn]] void Server::ReceiveClientData() {
    while (true) {
        udpReader.ReadMessage();

        switch (udpReader.GetMessageType()) {
            case Connect: {
                printf("Connecting...\n");
                udpWriter.AddClient(udpReader.GetIPAddress(), udpReader.GetPortNumber(), udpReader.GetClientId());
                namedPipeWriter.WriteMessage(udpReader.GetPackageId(), udpReader.GetMessageType(), udpReader.GetClientId());
                break;
            }
            case Disconnect: {
                printf("Disconnecting...\n");
                udpWriter.RemoveClient(udpReader.GetIPAddress(), udpReader.GetPortNumber(), udpReader.GetClientId());
                namedPipeWriter.WriteMessage(udpReader.GetPackageId(), udpReader.GetMessageType(), udpReader.GetClientId());
                break;
            }
            case Update: {
                namedPipeWriter.WriteMessage(udpReader.GetPackageId(), udpReader.GetMessageType(), udpReader.GetUpdateMessage());
                break;
            }
            case Spawn: {
                namedPipeWriter.WriteMessage(udpReader.GetPackageId(), udpReader.GetMessageType(), udpReader.GetSpawnMessage());
                break;
            }
            default: {
                // TODO
                break;
            }
        }
    }
}
