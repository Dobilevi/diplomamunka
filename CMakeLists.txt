
cmake_minimum_required(VERSION 3.29)

project(diplomamunka)

set(CMAKE_CXX_STANDARD 20)



add_executable(main
        main.cpp
        Server.cpp
        Server.h
        Game.cpp
        Game.h
        Player.cpp
        Player.h
        Projectile.cpp
        Projectile.h
        uudp.h
)

if(WIN32)
    target_link_libraries(main wsock32 ws2_32)
endif()
