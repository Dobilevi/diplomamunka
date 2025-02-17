
#include "Server.h"

#include <assert.h>
#include <future>
#include <iostream>
#include <sstream>
#include <string>

#include <winsock2.h>
#include <ws2tcpip.h>
#include <fcntl.h>

#include <unistd.h>
//#include <stdlib.h>
#include <cstring>
#include <iostream>

#include <sys/stat.h>
#include <sys/types.h>
#include <cerrno>

#include "uudp.h"

Server::Server() {
    assert(res == NO_ERROR);

    // Establish connection with the server instance
//    namedPipeWriter.WriteMessage(MessageType::Connect, static_cast<uint64_t>(321)); // TODO: test data

    receiveUpdateAsyncTask = std::async(&Server::ReceiveServerData, this);

    // Start listening for messages over the network
    receiveClientUpdateAsyncTask = std::async(&Server::ReceiveClientData, this);
}

Server::~Server() {
    closesocket(listenSocket);
    closesocket(sendSocket);
    WSACleanup();
}

[[noreturn]] void Server::ReceiveServerData() {
    MessageType messageType;
    uint64_t clientId;
    SpawnMessage spawnMessage;
    UpdateMessage updateMessage;

    while (true) {
        messageType = namedPipeReader.ReadMessageType();

        switch (messageType) {
            case MessageType::Connect:
            case MessageType::Disconnect:
                clientId = namedPipeReader.ReadConnectionMessage();
                udpWriter.MulticastMessage(messageType, clientId);
                break;
            case MessageType::Spawn:
                spawnMessage = namedPipeReader.ReadSpawnMessage();
                udpWriter.MulticastMessage(messageType, spawnMessage);
                break;
            case MessageType::Update:
                updateMessage = namedPipeReader.ReadUpdateMessage();
                udpWriter.MulticastMessage(messageType, updateMessage);
                break;
            default:
                break;
        }
    }
}

[[noreturn]] void Server::ReceiveClientData() {
    while (true) {
        switch (udpReader.ReadMessage()) {
            case Connect:
            case Disconnect:
                namedPipeWriter.WriteMessage(udpReader.GetMessageType(), udpReader.GetClientId());
                break;
            case Update:
                namedPipeWriter.WriteMessage(udpReader.GetMessageType(), udpReader.GetUpdateMessage());
                break;
            case Spawn:
            default:
                // TODO
                break;
        }
    }
}


//[[noreturn]] void Server::ReceiveClientData() {
//    fd_set readfds;
//
//    FD_ZERO(&readfds);
//    FD_SET(listenSocket, &readfds);
//
//    sockaddr address{AF_INET, "127.0.0.1"};
//
//    MessageType messageType;
//    uint64_t clientId;
//    UpdateMessage updateMessage;
//
//    int received;
//    char buffer[256];
//    int fromlen = sizeof(address);
//
//    float location[2] = {0.0f, 0.0f};
//
//
//    while(true) {
//
//        select(listenSocket + 1, &readfds, nullptr, nullptr, nullptr);
//
//        if (FD_ISSET(listenSocket, &readfds)) {
//            printf("Message received!\n");
//
//            received = recvfrom(listenSocket, buffer, sizeof(MessageType), 0, &address, &fromlen);
//
//            std::memcpy(&messageType, buffer, sizeof(MessageType));
//
//            switch (messageType) {
//                case Connect:
//                case Disconnect:
//                    std::memcpy(&clientId, buffer + sizeof(MessageType), sizeof(clientId));
//                    namedPipeWriter.WriteMessage(messageType, clientId);
//                    break;
//                case Spawn:
//                    break;
//                case Update:
//                    std::memcpy(&updateMessage, buffer + sizeof(MessageType), sizeof(updateMessage));
//                    namedPipeWriter.WriteMessage(messageType, updateMessage);
//                    break;
//                default:
//                    break;
//            }
//
//            //            printf("Received: %f, %f\n", reinterpret_cast<float*>(buffer)[0], reinterpret_cast<float*>(buffer)[1]);
//
////            game.players[0].SetPos(reinterpret_cast<float*>(buffer)[0], reinterpret_cast<float*>(buffer)[1]);
//        }
//        else {
//            printf("Timed out.\n");
//        }
//
//        FD_ZERO(&readfds);
//        FD_SET(listenSocket, &readfds);
//    }
//}

int Server::SendUpdates() {
    int result = 0;

    DataPacket packet;

    long long total_packets = 0;
    long long packets_received = 0;
    long long last_sequence = -1;
    long long out_of_order = 0;

    long long last_packet_arrival_time = -1;
    long long acc_delay = 0;

    fd_set writefds;

    FD_ZERO(&writefds);
    FD_SET(sendSocket, &writefds);

    sockaddr address{AF_INET, "127.0.0.1"};

    select(sendSocket + 1, nullptr, &writefds, nullptr, nullptr);

    if (FD_ISSET(sendSocket, &writefds)) {
//        float buf[2] = {game.players[0].GetPosX(), game.players[0].GetPosY()};
//        printf("%s\n", game.players[0].ToString().c_str());
//result = sendto(sendSocket, reinterpret_cast<char*>(buf), sizeof(float) * 2, 0, sendP->ai_addr, sendP->ai_addrlen);
        if (result == -1) {
            printf("Error: %d\n", errno);
        } else {
//            printf("Sent!\n");
        }
    }
    else {
        printf("Timed out.\n");
    }

//    FD_ZERO(&writefds);
//    FD_SET(sendSocket, &writefds);

    return result;
}

int Server::Start() {
    // Lobby phase
    gameRunning = false;

    int result;
    WSAData wsaData;
    result = WSAStartup(MAKEWORD(2, 2), &wsaData);
    assert(result == NO_ERROR);



    listenSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
    assert(listenSocket != INVALID_SOCKET);

    listenAddr.sin_addr.s_addr = inet_addr("127.0.0.1");
    listenAddr.sin_family = AF_INET;
    listenAddr.sin_port = htons(4000);

    // udpserver.cpp

    // --snip--
    // after setting the address.
    // And also, there's no sequence number initialization on the server size

    result = bind(listenSocket, (sockaddr*)&listenAddr, sizeof(listenAddr));
    assert(result != SOCKET_ERROR);


    // Send

    addrinfo hints, *servinfo;
    memset(&hints, 0, sizeof hints);
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

//    sendAddr.sin_addr.s_addr = inet_addr("127.0.0.1");
//    sendAddr.sin_family = AF_INET;
//    sendAddr.sin_port = htons(5000);

    // udpserver.cpp

    // --snip--
    // after setting the address.
    // And also, there's no sequence number initialization on the server size

//    result = bind(sendSocket, (sockaddr*)&sendAddr, sizeof(sendAddr));
//    assert(result != SOCKET_ERROR);

    return result;
}

bool Server::Test() {
//    static int n = 0;
//    static float x = -10;
//    static float y = -10;
//
//    namedPipeWriter.WriteMessage(MessageType::Update, UpdateMessage{321, x, 6.987654321, x, true}); // TODO: Test data
//    x += 0.01;
//
//    sleep(1);

    static float x = 1.0f;

    udpWriter.MulticastMessage(Update, UpdateMessage{365, x, 2.0f, x, true});
    x += 0.1f;

    sleep(1);
    printf("SENT!\n");

    return true;
}
