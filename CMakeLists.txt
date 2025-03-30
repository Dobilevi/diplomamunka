
cmake_minimum_required(VERSION 3.29)

project(diplomamunka)

set(CMAKE_CXX_STANDARD 20)

add_executable(main
        main.cpp
        Server.cpp
        Server.h
        Player.cpp
        Player.h
        NamedPipeReader.h
        NamedPipeReader.cpp
        NamedPipeWriter.h
        NamedPipeWriter.cpp
        UDPReader.h
        UDPReader.cpp
        UDPWriter.h
        UDPWriter.cpp
        Player.h
)

if(WIN32)
    target_link_libraries(main wsock32 ws2_32)
endif()
