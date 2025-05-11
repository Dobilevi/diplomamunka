
cmake_minimum_required(VERSION 3.29)

project(diplomamunka)

set(CMAKE_CXX_STANDARD 20)

set(CMAKE_EXE_LINKER_FLAGS "-static-libgcc -static-libstdc++ -static")

include_directories(.)
include_directories(Assets)
include_directories(NamedPipes)
include_directories(UDP)

add_executable(CppServer
        main.cpp
        Server.cpp
        Assets/Buffer.cpp
        Assets/NetworkHostConversion.cpp
        Assets/Player.cpp
        NamedPipes/NamedPipeReader.cpp
        NamedPipes/NamedPipeWriter.cpp
        UDP/UDPReader.cpp
        UDP/UDPWriter.cpp
)

if(WIN32)
    target_link_libraries(CppServer wsock32 ws2_32)
endif()
