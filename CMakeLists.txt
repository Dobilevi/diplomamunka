
cmake_minimum_required(VERSION 3.29)

project(diplomamunka)

set(CMAKE_CXX_STANDARD 20)

set(CMAKE_EXE_LINKER_FLAGS "-static-libgcc -static-libstdc++ -static")

include_directories(.)
include_directories(Assets)
include_directories(NamedPipes)
include_directories(UDP)

add_executable(main
        main.cpp
        Server.cpp
        Server.h
        Assets/Buffer.cpp
        Assets/Buffer.h
        Assets/NetworkHostConversion.cpp
        Assets/NetworkHostConversion.h
        Assets/Player.cpp
        Assets/Player.h
        NamedPipes/NamedPipeReader.h
        NamedPipes/NamedPipeReader.cpp
        NamedPipes/NamedPipeWriter.h
        NamedPipes/NamedPipeWriter.cpp
        UDP/UDPReader.h
        UDP/UDPReader.cpp
        UDP/UDPWriter.h
        UDP/UDPWriter.cpp
)

if(WIN32)
    target_link_libraries(main wsock32 ws2_32)
endif()
